namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>
    /// Client for Clothy.AI.FraudService. Methods never throw: any failure is logged as a warning
    /// and reported through the outcome, so payments keep working when the service is down.
    /// </summary>
    public interface IFraudScoringClient
    {
        Task<FraudScoreResult> ScoreAsync(FraudScoreRequest request, CancellationToken cancellationToken);

        /// <param name="txId">Order id used as txid when the payment was scored.</param>
        /// <param name="fraud">true: dispute opened; false: dispute won, label removed.</param>
        /// <param name="atUnixSeconds">Time of the label in the same scale as ts (Unix seconds).</param>
        Task<FraudLabelOutcome> LabelAsync(string txId, bool fraud, long atUnixSeconds, CancellationToken cancellationToken);
    }
}
