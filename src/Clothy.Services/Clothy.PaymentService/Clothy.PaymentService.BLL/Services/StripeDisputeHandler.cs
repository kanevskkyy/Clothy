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
    /// Idempotent: Stripe may deliver the same event again, a repeated event finds the status already set and does nothing.
    /// </summary>
    public class StripeDisputeHandler : IStripeDisputeHandler
    {
        public const string DISPUTE_CREATED = "charge.dispute.created";
        public const string DISPUTE_CLOSED = "charge.dispute.closed";

        private readonly PaymentDbContext dbContext;
        private readonly IStripePaymentDetailsProvider stripeDetails;
        private readonly IFraudScoringClient fraudClient;
        private readonly ILogger<StripeDisputeHandler> logger;

        public StripeDisputeHandler(PaymentDbContext dbContext, IStripePaymentDetailsProvider stripeDetails,
            IFraudScoringClient fraudClient, ILogger<StripeDisputeHandler> logger)
        {
            this.dbContext = dbContext;
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

            PaymentRecordEF? payment = await FindPaymentAsync(dispute.PaymentIntentId, cancellationToken);
            if (payment == null)
            {
                logger.LogWarning("No payment found for dispute {DisputeId}", dispute.Id);
                return;
            }

            if (stripeEvent.Type == DISPUTE_CREATED)
                await HandleCreatedAsync(dispute, payment, cancellationToken);
            else if (stripeEvent.Type == DISPUTE_CLOSED)
                await HandleClosedAsync(dispute, payment, stripeEvent.Created, cancellationToken);
        }

        private async Task HandleCreatedAsync(Dispute dispute, PaymentRecordEF payment, CancellationToken cancellationToken)
        {
            if (payment.Status != PaymentStatus.Paid)
            {
                // Disputed/DisputeWon/DisputeLost: already processed (repeated or late event)
                logger.LogInformation("Dispute {DisputeId} for Payment {PaymentId} skipped: status is already {Status}",
                    dispute.Id, payment.Id, payment.Status);
                return;
            }

            payment.Status = PaymentStatus.Disputed;
            payment.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Payment {PaymentId} of Order {OrderId} marked as Disputed", payment.Id, payment.OrderId);

            await fraudClient.LabelAsync(payment.OrderId.ToString(), true, ToUnixSeconds(dispute.Created), cancellationToken);
        }

        private async Task HandleClosedAsync(Dispute dispute, PaymentRecordEF payment, DateTime eventCreated, CancellationToken cancellationToken)
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

            if (payment.Status is not (PaymentStatus.Paid or PaymentStatus.Disputed))
            {
                logger.LogInformation("Dispute {DisputeId} for Payment {PaymentId} skipped: status is already {Status}",
                    dispute.Id, payment.Id, payment.Status);
                return;
            }

            bool createdWasMissed = payment.Status == PaymentStatus.Paid;
            payment.Status = target.Value;
            payment.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Payment {PaymentId} of Order {OrderId} marked as {Status}", payment.Id, payment.OrderId, payment.Status);

            string txId = payment.OrderId.ToString();
            if (target == PaymentStatus.DisputeWon)
            {
                // won: the payment was legitimate, remove the fraud label
                await fraudClient.LabelAsync(txId, false, ToUnixSeconds(eventCreated), cancellationToken);
            }
            else if (createdWasMissed)
            {
                // lost without a processed charge.dispute.created: set the label now, with the dispute time
                await fraudClient.LabelAsync(txId, true, ToUnixSeconds(dispute.Created), cancellationToken);
            }
        }

        /// <summary>
        /// The payment is found through PaymentIntent metadata (payment_id, order_id). Payments created before
        /// the metadata was copied to the PaymentIntent are found through their Checkout Session.
        /// </summary>
        private async Task<PaymentRecordEF?> FindPaymentAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<string, string>? metadata = await SafeAsync(
                () => stripeDetails.GetPaymentIntentMetadataAsync(paymentIntentId, cancellationToken));

            if (metadata == null || (!metadata.ContainsKey("payment_id") && !metadata.ContainsKey("order_id")))
            {
                metadata = await SafeAsync(() => stripeDetails.GetCheckoutSessionMetadataAsync(paymentIntentId, cancellationToken));
            }

            if (metadata == null) return null;

            if (metadata.TryGetValue("payment_id", out string? paymentIdStr) && Guid.TryParse(paymentIdStr, out Guid paymentId))
            {
                PaymentRecordEF? byId = await dbContext.PaymentRecords.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
                if (byId != null) return byId;
            }

            if (metadata.TryGetValue("order_id", out string? orderIdStr) && Guid.TryParse(orderIdStr, out Guid orderId))
            {
                // one order can have several payment attempts; the disputed one is the one that was paid
                return await dbContext.PaymentRecords
                    .Where(p => p.OrderId == orderId && p.Status != PaymentStatus.Pending)
                    .OrderByDescending(p => p.UpdatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return null;
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

        private static long ToUnixSeconds(DateTime utc)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }
    }
}
