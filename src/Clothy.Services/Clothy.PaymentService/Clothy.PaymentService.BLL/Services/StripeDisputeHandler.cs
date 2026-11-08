using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Clothy.PaymentService.BLL.Services
{
    public interface IStripeDisputeHandler
    {
        /// <summary>Handles a verified charge.dispute.created / charge.dispute.closed event.</summary>
        Task HandleAsync(Event stripeEvent, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Disputes update the payment status (Paid -> Disputed -> DisputeWon/DisputeLost) and send a label to
    /// FraudService. The order status in OrderService is not touched.
    ///
    /// Stripe does not guarantee event order and may deliver events in parallel or more than once:
    /// - a dispute means the money was taken, so a dispute for a Pending payment first runs the same
    ///   "paid" logic as checkout.session.completed (Paid, OrderPaidEvent, /score);
    /// - every status change is a compare-and-set (see PaymentRecordUpdater), so each step happens once;
    /// - the label is sent only after the /score attempt is over (FraudSync != None). If scoring is still
    ///   running in another webhook, that webhook sends the label when it finishes.
    /// </summary>
    public class StripeDisputeHandler : IStripeDisputeHandler
    {
        public const string DISPUTE_CREATED = "charge.dispute.created";
        public const string DISPUTE_CLOSED = "charge.dispute.closed";

        private enum LabelAction { None, SetFraud, RemoveFraud }

        private readonly PaymentDbContext dbContext;
        private readonly PaymentRecordUpdater updater;
        private readonly IStripePaymentLifecycle lifecycle;
        private readonly IStripePaymentDetailsProvider stripeDetails;
        private readonly IFraudScoringClient fraudClient;
        private readonly ILogger<StripeDisputeHandler> logger;

        public StripeDisputeHandler(PaymentDbContext dbContext, PaymentRecordUpdater updater, IStripePaymentLifecycle lifecycle,
            IStripePaymentDetailsProvider stripeDetails, IFraudScoringClient fraudClient, ILogger<StripeDisputeHandler> logger)
        {
            this.dbContext = dbContext;
            this.updater = updater;
            this.lifecycle = lifecycle;
            this.stripeDetails = stripeDetails;
            this.fraudClient = fraudClient;
            this.logger = logger;
        }

        public async Task HandleAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            if (stripeEvent.Data.Object is not Dispute dispute)
            {
                logger.LogWarning("Dispute object is null in Stripe event {EventId}", stripeEvent.Id);
                return;
            }

            if (string.IsNullOrEmpty(dispute.PaymentIntentId))
            {
                logger.LogWarning("Dispute {DisputeId} has no PaymentIntent", dispute.Id);
                return;
            }

            (PaymentRecordEF? payment, IReadOnlyDictionary<string, string>? sessionMetadata) =
                await FindPaymentAsync(dispute.PaymentIntentId, cancellationToken);
            if (payment == null)
            {
                logger.LogWarning("No payment found for dispute {DisputeId}", dispute.Id);
                return;
            }

            if (stripeEvent.Type != DISPUTE_CREATED && stripeEvent.Type != DISPUTE_CLOSED) return;

            // A dispute can arrive before checkout.session.completed (test card 4000 0000 0000 0259 opens it
            // in the same second). The payment did go through, so it is marked as paid here; whichever webhook
            // gets there first does it, the other one finds the payment no longer Pending.
            if (payment.Status == PaymentStatus.Pending)
            {
                await lifecycle.TryMarkPaidAsync(new StripePaidContext(
                    payment.Id, dispute.PaymentIntentId, null, sessionMetadata, stripeEvent.Created), cancellationToken);
            }

            if (stripeEvent.Type == DISPUTE_CREATED)
                await HandleCreatedAsync(dispute, payment.Id, cancellationToken);
            else
                await HandleClosedAsync(dispute, payment.Id, stripeEvent.Created, cancellationToken);
        }

        private async Task HandleCreatedAsync(Dispute dispute, Guid paymentId, CancellationToken cancellationToken)
        {
            (bool changed, LabelAction label, PaymentRecordEF? payment) = await updater.UpdateAsync(paymentId, p =>
            {
                if (p.Status != PaymentStatus.Paid) return (false, LabelAction.None, p);

                p.Status = PaymentStatus.Disputed;
                p.DisputedAt = dispute.Created;
                p.UpdatedAt = DateTime.UtcNow;
                // FraudSync == None: /score is still running in another webhook, which will send the label
                if (p.FraudSync != FraudSyncState.Scored) return (true, LabelAction.None, p);

                p.FraudSync = FraudSyncState.Labeled;
                return (true, LabelAction.SetFraud, p);
            }, cancellationToken);

            if (!changed)
            {
                // Disputed/DisputeWon/DisputeLost: already processed (repeated or late event)
                logger.LogInformation("Dispute {DisputeId} for Payment {PaymentId} skipped: status is already {Status}",
                    dispute.Id, paymentId, payment?.Status);
                return;
            }

            logger.LogInformation("Payment {PaymentId} of Order {OrderId} marked as Disputed", paymentId, payment!.OrderId);
            if (label == LabelAction.SetFraud) await lifecycle.SendFraudLabelAsync(payment, cancellationToken);
        }

        private async Task HandleClosedAsync(Dispute dispute, Guid paymentId, DateTime eventCreated, CancellationToken cancellationToken)
        {
            PaymentStatus? target = dispute.Status switch
            {
                "won" => PaymentStatus.DisputeWon,
                "lost" => PaymentStatus.DisputeLost,
                _ => null
            };

            if (target == null)
            {
                // e.g. warning_closed (inquiry without chargeback): nothing to record
                logger.LogInformation("Dispute {DisputeId} closed with status {DisputeStatus}; ignored", dispute.Id, dispute.Status);
                return;
            }

            (bool changed, LabelAction label, PaymentRecordEF? payment) = await updater.UpdateAsync(paymentId, p =>
            {
                if (p.Status is not (PaymentStatus.Paid or PaymentStatus.Disputed)) return (false, LabelAction.None, p);

                p.Status = target.Value;
                p.DisputedAt ??= dispute.Created;
                p.UpdatedAt = DateTime.UtcNow;

                if (target == PaymentStatus.DisputeWon && p.FraudSync == FraudSyncState.Labeled)
                {
                    // won: the payment was legitimate, remove the label that was sent
                    p.FraudSync = FraudSyncState.Unlabeled;
                    return (true, LabelAction.RemoveFraud, p);
                }

                if (target == PaymentStatus.DisputeLost && p.FraudSync == FraudSyncState.Scored)
                {
                    // lost without a label (charge.dispute.created was missed): set it now
                    p.FraudSync = FraudSyncState.Labeled;
                    return (true, LabelAction.SetFraud, p);
                }

                // FraudSync == None: scoring is still running and will label a DisputeLost payment itself
                return (true, LabelAction.None, p);
            }, cancellationToken);

            if (!changed)
            {
                logger.LogInformation("Dispute {DisputeId} for Payment {PaymentId} skipped: status is already {Status}",
                    dispute.Id, paymentId, payment?.Status);
                return;
            }

            logger.LogInformation("Payment {PaymentId} of Order {OrderId} marked as {Status}", paymentId, payment!.OrderId, payment.Status);

            if (label == LabelAction.SetFraud)
                await lifecycle.SendFraudLabelAsync(payment, cancellationToken);
            else if (label == LabelAction.RemoveFraud)
                await fraudClient.LabelAsync(payment.OrderId.ToString(), false, StripePaymentLifecycle.ToUnixSeconds(eventCreated), cancellationToken);
        }

        /// <summary>
        /// The payment is found through PaymentIntent metadata (payment_id, order_id). Payments created before
        /// the metadata was copied to the PaymentIntent are found through their Checkout Session.
        /// Also returns the Checkout Session metadata (device data) when it had to be read.
        /// </summary>
        private async Task<(PaymentRecordEF?, IReadOnlyDictionary<string, string>?)> FindPaymentAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<string, string>? metadata = await SafeAsync(
                () => stripeDetails.GetPaymentIntentMetadataAsync(paymentIntentId, cancellationToken));
            IReadOnlyDictionary<string, string>? sessionMetadata = null;

            if (metadata == null || (!metadata.ContainsKey("payment_id") && !metadata.ContainsKey("order_id")))
            {
                sessionMetadata = await SafeAsync(() => stripeDetails.GetCheckoutSessionMetadataAsync(paymentIntentId, cancellationToken));
                metadata = sessionMetadata;
            }

            if (metadata == null) return (null, null);

            PaymentRecordEF? payment = null;
            if (metadata.TryGetValue("payment_id", out string? paymentIdStr) && Guid.TryParse(paymentIdStr, out Guid paymentId))
            {
                payment = await dbContext.PaymentRecords.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
            }

            if (payment == null && metadata.TryGetValue("order_id", out string? orderIdStr) && Guid.TryParse(orderIdStr, out Guid orderId))
            {
                // one order can have several attempts: prefer the paid one, otherwise the latest attempt
                payment = (await dbContext.PaymentRecords.AsNoTracking()
                        .Where(p => p.OrderId == orderId)
                        .ToListAsync(cancellationToken))
                    .OrderByDescending(p => p.Status != PaymentStatus.Pending)
                    .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                    .FirstOrDefault();
            }

            // device data lives in the Checkout Session metadata; needed only if the dispute marks the payment as paid
            if (payment is { Status: PaymentStatus.Pending } && sessionMetadata == null)
            {
                sessionMetadata = await SafeAsync(() => stripeDetails.GetCheckoutSessionMetadataAsync(paymentIntentId, cancellationToken));
            }

            return (payment, sessionMetadata);
        }

        private async Task<IReadOnlyDictionary<string, string>?> SafeAsync(Func<Task<IReadOnlyDictionary<string, string>?>> call)
        {
            try
            {
                return await call();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning("Stripe metadata lookup for a dispute failed ({Error})", ex.GetType().Name);
                return null;
            }
        }
    }
}
