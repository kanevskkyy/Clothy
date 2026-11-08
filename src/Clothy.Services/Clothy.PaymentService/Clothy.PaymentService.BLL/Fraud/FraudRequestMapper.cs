using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>Device data sent by the storefront with the payment (all fields optional).</summary>
    public record FraudDeviceData(string? DeviceType, string? DeviceInfo, string? Os, string? Browser, string? Screen);

    /// <summary>Everything about a paid order that is needed for POST /score.</summary>
    public record FraudPaymentData(
        Guid OrderId,
        DateTime PaidAtUtc,
        decimal Amount,
        string CardFingerprint,
        string? CardBrand,
        string? CardFunding,
        string? BillingPostalCode,
        string? Email,
        FraudDeviceData? Device);

    /// <summary>
    /// Maps Stripe payment data to the IEEE-CIS fields the model was trained on.
    /// The mapping is approximate (see README): the model never saw Stripe data.
    /// </summary>
    public static class FraudRequestMapper
    {
        /// <summary>ProductCD in IEEE-CIS; "W" is the most common code and stands for regular web purchases.</summary>
        public const string PRODUCT_CODE = "W";

        public static FraudScoreRequest Build(FraudPaymentData data)
        {
            return new FraudScoreRequest
            {
                TxId = data.OrderId.ToString(),
                Ts = new DateTimeOffset(DateTime.SpecifyKind(data.PaidAtUtc, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                TransactionAmt = (double)data.Amount,
                Card1 = CardFingerprintToCard1(data.CardFingerprint),
                ProductCD = PRODUCT_CODE,
                Card4 = MapBrand(data.CardBrand),
                Card6 = MapFunding(data.CardFunding),
                Addr1 = PostalCodeToAddr1(data.BillingPostalCode),
                PEmailDomain = EmailDomain(data.Email),
                DeviceType = Clean(data.Device?.DeviceType),
                DeviceInfo = Clean(data.Device?.DeviceInfo),
                Id30 = Clean(data.Device?.Os),
                Id31 = Clean(data.Device?.Browser),
                Id33 = Clean(data.Device?.Screen)
            };
        }

        /// <summary>
        /// card1 is the key of the card history (1/7/30-day windows, card+address and card+email pairs),
        /// so it has to be the same number for the same card on every payment. Stripe gives a stable
        /// card.fingerprint (same card number = same fingerprint within one Stripe account).
        /// We take the first 8 bytes of SHA-256 of the fingerprint and keep the top 53 bits:
        /// any integer below 2^53 is stored in a double exactly, so the value survives JSON, Python float
        /// and SQLite REAL without rounding and equality lookups in the history stay correct.
        /// 53 bits make collisions between different cards negligible for a shop.
        /// The value is not in the IEEE-CIS card1 range (1000-18396): as a numeric feature it carries no
        /// meaning for the model, it only links the history of one card.
        /// </summary>
        public static double CardFingerprintToCard1(string fingerprint)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint));
            // big-endian read: the same number on any CPU
            ulong value = BinaryPrimitives.ReadUInt64BigEndian(hash) >> 11;
            return value;
        }

        /// <summary>Stripe brand -> IEEE-CIS card4 (visa, mastercard, american express, discover).</summary>
        public static string? MapBrand(string? brand)
        {
            string? b = Clean(brand)?.ToLowerInvariant();
            return b switch
            {
                null or "unknown" => null,
                "amex" => "american express",
                _ => b
            };
        }

        /// <summary>Stripe funding -> IEEE-CIS card6 (credit, debit); prepaid/unknown have no counterpart.</summary>
        public static string? MapFunding(string? funding)
        {
            string? f = Clean(funding)?.ToLowerInvariant();
            return f is "credit" or "debit" ? f : null;
        }

        /// <summary>addr1 is numeric in IEEE-CIS: the postal code as a number when it has only digits, otherwise null.</summary>
        public static double? PostalCodeToAddr1(string? postalCode)
        {
            string? p = Clean(postalCode);
            if (p == null || p.Length > 15 || !p.All(char.IsAsciiDigit)) return null;
            return double.Parse(p, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        public static string? EmailDomain(string? email)
        {
            string? e = Clean(email);
            if (e == null) return null;
            int at = e.LastIndexOf('@');
            if (at < 0 || at == e.Length - 1) return null;
            return e[(at + 1)..].ToLowerInvariant();
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
