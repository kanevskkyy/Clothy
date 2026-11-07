using Clothy.PaymentService.BLL.Config;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>Card data of the charge behind a PaymentIntent. Contains personal data: never log it.</summary>
    public record StripeChargeDetails(
        string? CardFingerprint,
        string? CardBrand,
        string? CardFunding,
        string? BillingPostalCode,
        string? BillingEmail,
        DateTime? ChargeCreatedUtc,
        string? Currency = null);

    /// <summary>Reads from the Stripe API what webhook payloads do not contain.</summary>
    public interface IStripePaymentDetailsProvider
    {
        Task<StripeChargeDetails?> GetChargeDetailsAsync(string paymentIntentId, CancellationToken cancellationToken);

        Task<IReadOnlyDictionary<string, string>?> GetPaymentIntentMetadataAsync(string paymentIntentId, CancellationToken cancellationToken);

        /// <summary>
        /// Metadata of the Checkout Session that created the PaymentIntent. Needed for payments made
        /// before order_id was copied to PaymentIntent metadata.
        /// </summary>
        Task<IReadOnlyDictionary<string, string>?> GetCheckoutSessionMetadataAsync(string paymentIntentId, CancellationToken cancellationToken);
    }

    public class StripePaymentDetailsProvider : IStripePaymentDetailsProvider
    {
        private readonly CardSettings settings;

        public StripePaymentDetailsProvider(IOptions<CardSettings> settings)
        {
            this.settings = settings.Value;
        }

        // Own client instead of the static StripeConfiguration.ApiKey:
        // a webhook can arrive before any payment was created in this process.
        private StripeClient CreateClient() => new StripeClient(settings.ApiKey);

        public async Task<StripeChargeDetails?> GetChargeDetailsAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            PaymentIntentService service = new PaymentIntentService(CreateClient());
            PaymentIntent intent = await service.GetAsync(paymentIntentId,
                new PaymentIntentGetOptions { Expand = new List<string> { "latest_charge" } },
                cancellationToken: cancellationToken);

            Charge? charge = intent.LatestCharge;
            if (charge == null) return null;

            ChargePaymentMethodDetailsCard? card = charge.PaymentMethodDetails?.Card;
            return new StripeChargeDetails(
                card?.Fingerprint,
                card?.Brand,
                card?.Funding,
                charge.BillingDetails?.Address?.PostalCode,
                charge.BillingDetails?.Email,
                charge.Created,
                charge.Currency);
        }

        public async Task<IReadOnlyDictionary<string, string>?> GetPaymentIntentMetadataAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            PaymentIntentService service = new PaymentIntentService(CreateClient());
            PaymentIntent intent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            return intent.Metadata;
        }

        public async Task<IReadOnlyDictionary<string, string>?> GetCheckoutSessionMetadataAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            SessionService service = new SessionService(CreateClient());
            StripeList<Session> sessions = await service.ListAsync(
                new SessionListOptions { PaymentIntent = paymentIntentId, Limit = 1 },
                cancellationToken: cancellationToken);
            return sessions.Data.FirstOrDefault()?.Metadata;
        }
    }
}
