using Clothy.PaymentService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Clothy.PaymentService.BLL.Fraud
{
    public interface IStripeFraudScorer
    {
        /// <summary>
        /// Scores a paid payment. Never throws: the payment is already saved and must not fail.
        /// </summary>
        /// <param name="payment">The paid payment (order id and total).</param>
        /// <param name="paymentIntentId">Stripe PaymentIntent; card data is read from its latest charge.</param>
        /// <param name="customerEmail">Checkout email if known; otherwise the billing email of the charge.</param>
        /// <param name="sessionMetadata">Checkout Session metadata with optional device data.</param>
        /// <param name="eventCreatedUtc">Fallback payment time when the charge has none.</param>
        Task<FraudScoreOutcome> ScorePaidPaymentAsync(PaymentRecordEF payment, string? paymentIntentId, string? customerEmail,
            IReadOnlyDictionary<string, string>? sessionMetadata, DateTime eventCreatedUtc, CancellationToken cancellationToken);
    }

    public class StripeFraudScorer : IStripeFraudScorer
    {
        /// <summary>
        /// The model was trained on IEEE-CIS amounts in US dollars. Checkout charges in USD
        /// (StripeService.PAY_CURRENCY), so the order total is sent as is. A charge in any other currency
        /// is not scored rather than sent with an amount the model would misread.
        /// </summary>
        public const string MODEL_CURRENCY = "usd";

        private readonly IFraudScoringClient fraudClient;
        private readonly IStripePaymentDetailsProvider stripeDetails;
        private readonly ILogger<StripeFraudScorer> logger;

        public StripeFraudScorer(IFraudScoringClient fraudClient, IStripePaymentDetailsProvider stripeDetails, ILogger<StripeFraudScorer> logger)
        {
            this.fraudClient = fraudClient;
            this.stripeDetails = stripeDetails;
            this.logger = logger;
        }

        public async Task<FraudScoreOutcome> ScorePaidPaymentAsync(PaymentRecordEF payment, string? paymentIntentId, string? customerEmail,
            IReadOnlyDictionary<string, string>? sessionMetadata, DateTime eventCreatedUtc, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentIntentId))
                {
                    logger.LogWarning("Order {OrderId} is not fraud-scored: no PaymentIntent", payment.OrderId);
                    return FraudScoreOutcome.NotScored;
                }

                StripeChargeDetails? charge = await stripeDetails.GetChargeDetailsAsync(paymentIntentId, cancellationToken);
                if (charge?.CardFingerprint == null)
                {
                    logger.LogWarning("Order {OrderId} is not fraud-scored: no card fingerprint in the charge", payment.OrderId);
                    return FraudScoreOutcome.NotScored;
                }

                if (charge.Currency != null && !string.Equals(charge.Currency, MODEL_CURRENCY, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Order {OrderId} is not fraud-scored: charge currency {Currency} is not USD, the model expects USD amounts",
                        payment.OrderId, charge.Currency);
                    return FraudScoreOutcome.NotScored;
                }

                FraudPaymentData data = new FraudPaymentData(
                    payment.OrderId,
                    charge.ChargeCreatedUtc ?? eventCreatedUtc,
                    payment.Price,
                    charge.CardFingerprint,
                    charge.CardBrand,
                    charge.CardFunding,
                    charge.BillingPostalCode,
                    customerEmail ?? charge.BillingEmail,
                    FraudDeviceMetadata.From(sessionMetadata));

                FraudScoreResult result = await fraudClient.ScoreAsync(FraudRequestMapper.Build(data), cancellationToken);

                switch (result.Outcome)
                {
                    case FraudScoreOutcome.Scored:
                        logger.LogInformation("Order {OrderId} fraud-scored: flag={Flag}", payment.OrderId, result.Response!.Flag);
                        break;
                    case FraudScoreOutcome.AlreadyScored:
                        logger.LogInformation("Order {OrderId} was already fraud-scored", payment.OrderId);
                        break;
                    default:
                        logger.LogWarning("Order {OrderId} is marked as not fraud-scored", payment.OrderId);
                        break;
                }

                return result.Outcome;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Order {OrderId} is not fraud-scored: {Error}", payment.OrderId, ex.GetType().Name);
                return FraudScoreOutcome.NotScored;
            }
        }
    }
}
