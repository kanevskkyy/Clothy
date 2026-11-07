using Clothy.PaymentService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;

namespace Clothy.PaymentService.BLL.Fraud
{
    public interface IStripeFraudScorer
    {
        /// <summary>Scores a paid Checkout Session. Never throws: the payment is already saved and must not fail.</summary>
        Task<FraudScoreOutcome> ScorePaidSessionAsync(Session session, PaymentRecordEF payment, DateTime eventCreatedUtc, CancellationToken cancellationToken);
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

        public async Task<FraudScoreOutcome> ScorePaidSessionAsync(Session session, PaymentRecordEF payment, DateTime eventCreatedUtc, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(session.PaymentIntentId))
                {
                    logger.LogWarning("Order {OrderId} is not fraud-scored: session has no PaymentIntent", payment.OrderId);
                    return FraudScoreOutcome.NotScored;
                }

                StripeChargeDetails? charge = await stripeDetails.GetChargeDetailsAsync(session.PaymentIntentId, cancellationToken);
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
                    session.CustomerDetails?.Email ?? charge.BillingEmail,
                    FraudDeviceMetadata.From(session.Metadata));

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
