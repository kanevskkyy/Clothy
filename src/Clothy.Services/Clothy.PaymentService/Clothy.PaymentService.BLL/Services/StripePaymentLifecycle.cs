using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.Domain.Entities;
using Clothy.Shared.Events.PaymentEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.PaymentService.BLL.Services
{
    /// <summary>What is known about a successful Stripe payment when it is marked as paid.</summary>
    public record StripePaidContext(
        Guid PaymentId,
        string? PaymentIntentId,
        string? CustomerEmail,
        IReadOnlyDictionary<string, string>? SessionMetadata,
        DateTime EventCreatedUtc);

    public interface IStripePaymentLifecycle
    {
        /// <summary>
        /// Pending -> Paid, then OrderPaidEvent and fraud scoring. Used by checkout.session.completed and by a
        /// dispute that arrives first. Only the call that actually moves the payment out of Pending does the
        /// rest, so OrderPaidEvent and /score happen once.
        /// </summary>
        /// <returns>true if this call marked the payment as paid.</returns>
        Task<bool> TryMarkPaidAsync(StripePaidContext context, CancellationToken cancellationToken);

        /// <summary>Sends the fraud label of a disputed payment (dispute creation time).</summary>
        Task SendFraudLabelAsync(PaymentRecordEF payment, CancellationToken cancellationToken);
    }

    public class StripePaymentLifecycle : IStripePaymentLifecycle
    {
        private readonly PaymentRecordUpdater updater;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IStripeFraudScorer fraudScorer;
        private readonly IFraudScoringClient fraudClient;
        private readonly ILogger<StripePaymentLifecycle> logger;

        public StripePaymentLifecycle(PaymentRecordUpdater updater, IPublishEndpoint publishEndpoint,
            IStripeFraudScorer fraudScorer, IFraudScoringClient fraudClient, ILogger<StripePaymentLifecycle> logger)
        {
            this.updater = updater;
            this.publishEndpoint = publishEndpoint;
            this.fraudScorer = fraudScorer;
            this.fraudClient = fraudClient;
            this.logger = logger;
        }

        public async Task<bool> TryMarkPaidAsync(StripePaidContext context, CancellationToken cancellationToken)
        {
            PaymentRecordEF? paid = await updater.UpdateAsync(context.PaymentId, p =>
            {
                // != Pending: already paid or disputed, a repeated or late event changes nothing
                if (p.Status != PaymentStatus.Pending) return null;
                p.Status = PaymentStatus.Paid;
                p.UpdatedAt = DateTime.UtcNow;
                return p;
            }, cancellationToken);

            if (paid == null)
            {
                logger.LogInformation("Payment {PaymentId} already marked as Paid", context.PaymentId);
                return false;
            }

            await publishEndpoint.Publish(new OrderPaidEvent { OrderId = paid.OrderId }, cancellationToken);
            logger.LogInformation("Payment {PaymentId} marked as Paid via webhook", paid.Id);

            // Scoring never throws: if FraudService is down the order simply stays "not scored".
            await fraudScorer.ScorePaidPaymentAsync(paid, context.PaymentIntentId, context.CustomerEmail,
                context.SessionMetadata, context.EventCreatedUtc, cancellationToken);

            // The /score attempt is over. If a dispute arrived meanwhile, it left the label to us,
            // because a label for a transaction FraudService does not know yet would be rejected with 404.
            PaymentRecordEF? toLabel = await updater.UpdateAsync(paid.Id, p =>
            {
                if (p.FraudSync != FraudSyncState.None) return null;
                if (p.Status is PaymentStatus.Disputed or PaymentStatus.DisputeLost)
                {
                    p.FraudSync = FraudSyncState.Labeled;
                    return p;
                }
                p.FraudSync = FraudSyncState.Scored;
                return null;
            }, cancellationToken);

            if (toLabel != null) await SendFraudLabelAsync(toLabel, cancellationToken);
            return true;
        }

        public async Task SendFraudLabelAsync(PaymentRecordEF payment, CancellationToken cancellationToken)
        {
            DateTime at = payment.DisputedAt ?? payment.UpdatedAt ?? DateTime.UtcNow;
            await fraudClient.LabelAsync(payment.OrderId.ToString(), true, ToUnixSeconds(at), cancellationToken);
        }

        public static long ToUnixSeconds(DateTime utc)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }
    }
}
