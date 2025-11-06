using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs.ClotheItem;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.Aggregator.Aggregate.Services
{
    public class ClotheAggregateService : IClotheAggregateService
    {
        private IClotheGrpcClient clotheGrpcClient;
        private IReviewGrpcClient reviewGrpcClient;
        private ILogger<ClotheAggregateService> logger;

        public ClotheAggregateService(IClotheGrpcClient clotheGrpcClient, IReviewGrpcClient reviewGrpcClient, ILogger<ClotheAggregateService> logger)
        {
            this.clotheGrpcClient = clotheGrpcClient;
            this.reviewGrpcClient = reviewGrpcClient;
            this.logger = logger;
        }

        public async Task<ClotheDetailFullDTO> GetFullClotheDetailAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting aggregation for ClotheId: {ClotheId}", clotheId);

            try
            {
                var clotheTask = clotheGrpcClient.GetClotheByIdAsync(clotheId.ToString());
                var reviewsTask = reviewGrpcClient.GetReviewsByClotheIdAsync(clotheId, cancellationToken);
                var questionsTask = reviewGrpcClient.GetQuestionsAndAnswersByClotheIdAsync(clotheId, cancellationToken);
                var statsTask = reviewGrpcClient.GetStatisticsByClotheIdAsync(clotheId, cancellationToken);

                await Task.WhenAll(clotheTask, reviewsTask, questionsTask, statsTask);

                ClotheDetailGrpcResponse clotheDetail = clotheTask.Result;
                ReviewsListGrpcResponse reviews = reviewsTask.Result;
                QuestionsListGrpcResponse questions = questionsTask.Result;
                ReviewStatisticGrpcResponse stats = statsTask.Result;

                ClotheDetailFullDTO clotheDetailFullDTO = new ClotheDetailFullDTO
                {
                    ClotheDetailDTO = clotheDetail,
                    Reviews = reviews.Reviews.ToList(),
                    Questions = questions.Questions.ToList(),
                    Statistics = stats
                };

                logger.LogInformation("Successfully aggregated full detail for ClotheId: {ClotheId}", clotheId);

                return clotheDetailFullDTO;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while aggregating data for ClotheId: {ClotheId}", clotheId);
                throw;
            }
        }
    }
}
