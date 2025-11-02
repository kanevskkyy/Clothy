using System.Text.Json;
using Clothy.Aggregator.DTOs.Questions;
using Clothy.Aggregator.DTOs.Reviews;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.Shared.Helpers;

namespace Clothy.Aggregator.Clients
{
    public class ReviewClient
    {
        private HttpClient httpClient;
        private ILogger<ReviewClient> logger;

        public ReviewClient(HttpClient httpClient, ILogger<ReviewClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<ReviewStatisticsDTO?> GetStatisticsAsync(Guid clotheItemId, CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync($"/api/reviews/statistics/{clotheItemId}", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch statistics for ClotheItemId {Id}. Status: {StatusCode}", clotheItemId, response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<ReviewStatisticsDTO>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching statistics for ClotheItemId {Id}", clotheItemId);
                return null;
            }
        }

        public async Task<List<ReviewResponseDTO>?> GetReviewsAsync(Guid clotheItemId, CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync($"/api/reviews?ClotheItemId={clotheItemId}", ct);
                if (!response.IsSuccessStatusCode) return null;

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                PagedList<ReviewResponseDTO>? paged =
                    await response.Content.ReadFromJsonAsync<PagedList<ReviewResponseDTO>>(options, ct);

                return paged?.Items;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching reviews for ClotheItemId {Id}", clotheItemId);
                return null;
            }
        }

        public async Task<List<QuestionResponseDTO>?> GetQuestionsAsync(Guid clotheItemId, CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync($"/api/questions?ClotheItemId={clotheItemId}", ct);
                if (!response.IsSuccessStatusCode) return null;

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                PagedList<QuestionResponseDTO>? paged = await response.Content.ReadFromJsonAsync<PagedList<QuestionResponseDTO>>(options, ct);
                return paged?.Items;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching questions for ClotheItemId {Id}", clotheItemId);
                return null;
            }
        }
    }
}
