using System.Text.Json.Serialization;

namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>
    /// Request body for POST /score. Field names follow the IEEE-CIS schema used by the fraud model.
    /// </summary>
    public class FraudScoreRequest
    {
        [JsonPropertyName("txid")] public string TxId { get; set; } = string.Empty;
        [JsonPropertyName("ts")] public long Ts { get; set; }
        [JsonPropertyName("TransactionAmt")] public double TransactionAmt { get; set; }
        [JsonPropertyName("card1")] public double Card1 { get; set; }
        [JsonPropertyName("ProductCD")] public string? ProductCD { get; set; }
        [JsonPropertyName("card4")] public string? Card4 { get; set; }
        [JsonPropertyName("card6")] public string? Card6 { get; set; }
        [JsonPropertyName("addr1")] public double? Addr1 { get; set; }
        [JsonPropertyName("P_emaildomain")] public string? PEmailDomain { get; set; }
        [JsonPropertyName("DeviceType")] public string? DeviceType { get; set; }
        [JsonPropertyName("DeviceInfo")] public string? DeviceInfo { get; set; }
        [JsonPropertyName("id_30")] public string? Id30 { get; set; }
        [JsonPropertyName("id_31")] public string? Id31 { get; set; }
        [JsonPropertyName("id_33")] public string? Id33 { get; set; }
    }

    public class FraudTopFeature
    {
        [JsonPropertyName("feature")] public string Feature { get; set; } = string.Empty;
        [JsonPropertyName("value")] public double? Value { get; set; }
        [JsonPropertyName("contribution")] public double Contribution { get; set; }
    }

    public class FraudRingSignal
    {
        [JsonPropertyName("suspect")] public bool Suspect { get; set; }
        [JsonPropertyName("active")] public bool Active { get; set; }
        [JsonPropertyName("other_cards_30d")] public int? OtherCards30d { get; set; }
        [JsonPropertyName("share_new")] public double? ShareNew { get; set; }
    }

    /// <summary>Response of POST /score (only the fields Clothy uses).</summary>
    public class FraudScoreResponse
    {
        [JsonPropertyName("txid")] public string TxId { get; set; } = string.Empty;
        [JsonPropertyName("score")] public double Score { get; set; }
        [JsonPropertyName("flag")] public bool Flag { get; set; }
        [JsonPropertyName("threshold")] public double Threshold { get; set; }
        [JsonPropertyName("top_features")] public List<FraudTopFeature> TopFeatures { get; set; } = new();
        [JsonPropertyName("ring")] public FraudRingSignal? Ring { get; set; }
    }

    public enum FraudScoreOutcome
    {
        /// <summary>Scored now; the decision is stored in the fraud service.</summary>
        Scored,
        /// <summary>409 and the transaction already exists in the fraud service.</summary>
        AlreadyScored,
        /// <summary>Service unavailable, timeout, error or rejected request: order stays "not scored".</summary>
        NotScored
    }

    public record FraudScoreResult(FraudScoreOutcome Outcome, FraudScoreResponse? Response = null);

    public enum FraudLabelOutcome
    {
        Labeled,
        /// <summary>404: the order was never scored, so there is nothing to label.</summary>
        UnknownTransaction,
        Failed
    }
}
