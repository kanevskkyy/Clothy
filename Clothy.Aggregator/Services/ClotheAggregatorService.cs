using Clothy.Aggregator.Clients;
using Clothy.Aggregator.DTOs.ClotheItem;
using Clothy.Aggregator.DTOs.Questions;
using Clothy.Aggregator.DTOs.Reviews;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;

namespace Clothy.Aggregator.Services
{
    public class ClotheAggregatorService
    {
        private CatalogClient catalogClient;
        private ReviewClient reviewClient;
        private ILogger<ClotheAggregatorService> logger;

        public ClotheAggregatorService(CatalogClient catalogClient, ReviewClient reviewClient, ILogger<ClotheAggregatorService> logger)
        {
            this.catalogClient = catalogClient;
            this.reviewClient = reviewClient;
            this.logger = logger;
        }

        public async Task<ClotheDetailFullDTO?> GetClotheFullDetailAsync(Guid clotheItemId, CancellationToken ct)
        {
            ClotheDetailDTO? clothe = await catalogClient.GetClotheByIdAsync(clotheItemId, ct);
            if (clothe == null)
            {
                logger.LogWarning("Clothe with id {Id} not found", clotheItemId);
                return null;
            }

            List<ReviewResponseDTO> reviews = await reviewClient.GetReviewsAsync(clotheItemId, ct) ?? new List<ReviewResponseDTO>();
            ReviewStatisticsDTO? stats = await reviewClient.GetStatisticsAsync(clotheItemId, ct);
            List<QuestionResponseDTO> questions = await reviewClient.GetQuestionsAsync(clotheItemId, ct) ?? new List<QuestionResponseDTO>();

            return new ClotheDetailFullDTO
            {
                ClotheDetailDTO = clothe,
                Reviews = reviews,
                Statistics = stats,
                Questions = questions
            };
        }
    }
}
