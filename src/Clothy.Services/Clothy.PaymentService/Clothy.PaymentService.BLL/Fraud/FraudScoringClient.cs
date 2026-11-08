using Clothy.PaymentService.BLL.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clothy.PaymentService.BLL.Fraud
{
    /// <summary>
    /// Typed HttpClient for Clothy.AI.FraudService.
    /// The circuit breaker is configured at registration (Program.cs); the hard time limit lives here,
    /// because the default resilience handler from ServiceDefaults would otherwise retry for up to 30 s.
    /// Logs contain only the order id and status codes, never card, email, address or device fields.
    /// </summary>
    public class FraudScoringClient : IFraudScoringClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient httpClient;
        private readonly ILogger<FraudScoringClient> logger;
        private readonly TimeSpan timeout;

        public FraudScoringClient(HttpClient httpClient, ILogger<FraudScoringClient> logger, IOptions<FraudSettings> settings)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            timeout = settings.Value.Timeout;
        }

        public async Task<FraudScoreResult> ScoreAsync(FraudScoreRequest request, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsJsonAsync("score", request, JsonOptions, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    FraudScoreResponse? body = await response.Content.ReadFromJsonAsync<FraudScoreResponse>(JsonOptions, cts.Token);
                    if (body == null)
                    {
                        logger.LogWarning("FraudService returned an empty body for Order {OrderId}; order is not scored", request.TxId);
                        return new FraudScoreResult(FraudScoreOutcome.NotScored);
                    }

                    return new FraudScoreResult(FraudScoreOutcome.Scored, body);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    return await ResolveConflictAsync(request.TxId, cts.Token);
                }

                logger.LogWarning("FraudService returned {StatusCode} for Order {OrderId}; order is not scored",
                    (int)response.StatusCode, request.TxId);
                return new FraudScoreResult(FraudScoreOutcome.NotScored);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("FraudService did not answer within {Timeout} s for Order {OrderId}; order is not scored",
                    timeout.TotalSeconds, request.TxId);
                return new FraudScoreResult(FraudScoreOutcome.NotScored);
            }
            catch (Exception ex)
            {
                logger.LogWarning("FraudService call failed for Order {OrderId} ({Error}); order is not scored",
                    request.TxId, ex.GetType().Name);
                return new FraudScoreResult(FraudScoreOutcome.NotScored);
            }
        }

        /// <summary>
        /// 409 means either a duplicate txid (already scored) or a ts earlier than the last stored one
        /// (a late webhook). Only GET /tx tells them apart.
        /// </summary>
        private async Task<FraudScoreResult> ResolveConflictAsync(string txId, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await httpClient.GetAsync($"tx/{Uri.EscapeDataString(txId)}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Order {OrderId} is already scored by FraudService", txId);
                return new FraudScoreResult(FraudScoreOutcome.AlreadyScored);
            }

            logger.LogWarning("FraudService rejected Order {OrderId} with 409 (payment time is earlier than the last scored one); order is not scored",
                txId);
            return new FraudScoreResult(FraudScoreOutcome.NotScored);
        }

        public async Task<FraudLabelOutcome> LabelAsync(string txId, bool fraud, long atUnixSeconds, CancellationToken cancellationToken)
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                var body = new { txid = txId, fraud, at = atUnixSeconds };
                using HttpResponseMessage response = await httpClient.PostAsJsonAsync("labels", body, JsonOptions, cts.Token);

                if (response.IsSuccessStatusCode) return FraudLabelOutcome.Labeled;

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning("FraudService does not know Order {OrderId} (it was not scored); label is skipped", txId);
                    return FraudLabelOutcome.UnknownTransaction;
                }

                logger.LogWarning("FraudService returned {StatusCode} for label of Order {OrderId}", (int)response.StatusCode, txId);
                return FraudLabelOutcome.Failed;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("FraudService did not answer within {Timeout} s for label of Order {OrderId}", timeout.TotalSeconds, txId);
                return FraudLabelOutcome.Failed;
            }
            catch (Exception ex)
            {
                logger.LogWarning("FraudService label call failed for Order {OrderId} ({Error})", txId, ex.GetType().Name);
                return FraudLabelOutcome.Failed;
            }
        }
    }
}
