namespace Clothy.PaymentService.BLL.Config
{
    public class FraudSettings
    {
        /// <summary>Base URL of Clothy.AI.FraudService (FRAUD__URL).</summary>
        public string BaseUrl { get; set; } = "http://localhost:8097";

        /// <summary>Hard limit for one call to the fraud service, including retries.</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);
    }
}
