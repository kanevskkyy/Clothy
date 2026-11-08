namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>
    /// Device data travels from the storefront to the webhook through Checkout Session metadata,
    /// so neither OrderService nor the payment table needs new columns.
    /// </summary>
    public static class FraudDeviceMetadata
    {
        public const string DEVICE_TYPE = "device_type";
        public const string DEVICE_INFO = "device_info";
        public const string DEVICE_OS = "device_os";
        public const string DEVICE_BROWSER = "device_browser";
        public const string DEVICE_SCREEN = "device_screen";

        // Stripe allows up to 500 characters per metadata value; device strings are much shorter.
        private const int MAX_LENGTH = 100;

        public static void AddTo(IDictionary<string, string> metadata, FraudDeviceData? device)
        {
            if (device == null) return;
            Add(metadata, DEVICE_TYPE, device.DeviceType);
            Add(metadata, DEVICE_INFO, device.DeviceInfo);
            Add(metadata, DEVICE_OS, device.Os);
            Add(metadata, DEVICE_BROWSER, device.Browser);
            Add(metadata, DEVICE_SCREEN, device.Screen);
        }

        public static FraudDeviceData? From(IReadOnlyDictionary<string, string>? metadata)
        {
            if (metadata == null) return null;
            FraudDeviceData device = new FraudDeviceData(
                Get(metadata, DEVICE_TYPE), Get(metadata, DEVICE_INFO), Get(metadata, DEVICE_OS),
                Get(metadata, DEVICE_BROWSER), Get(metadata, DEVICE_SCREEN));
            return device == new FraudDeviceData(null, null, null, null, null) ? null : device;
        }

        private static void Add(IDictionary<string, string> metadata, string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string trimmed = value.Trim();
            metadata[key] = trimmed.Length > MAX_LENGTH ? trimmed.Substring(0, MAX_LENGTH) : trimmed;
        }

        private static string? Get(IReadOnlyDictionary<string, string> metadata, string key)
        {
            return metadata.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value) ? value : null;
        }
    }
}
