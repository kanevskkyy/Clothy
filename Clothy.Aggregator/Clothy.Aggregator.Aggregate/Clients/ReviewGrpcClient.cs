using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class ReviewGrpcClient : IReviewGrpcClient
    {
        private ReviewServiceGrpc.ReviewServiceGrpcClient client;
        private ILogger<ReviewGrpcClient> logger;

        public ReviewGrpcClient(ReviewServiceGrpc.ReviewServiceGrpcClient client, ILogger<ReviewGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<ReviewsListGrpcResponse> GetReviewsByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching reviews for ClotheId: {ClotheId}", clotheId);
                ReviewClotheIdGrpcRequest request = new ReviewClotheIdGrpcRequest 
                { 
                    Id = clotheId.ToString() 
                };
                ReviewsListGrpcResponse response = await client.GetReviewsByClotheIdAsync(request, cancellationToken: cancellationToken);
                logger.LogInformation("Successfully fetched reviews for ClotheId: {ClotheId}", clotheId);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching reviews for ClotheId: {ClotheId}", clotheId);
                throw;
            }
        }

        public async Task<QuestionsListGrpcResponse> GetQuestionsAndAnswersByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching Q&A for ClotheId: {ClotheId}", clotheId);
                ReviewClotheIdGrpcRequest request = new ReviewClotheIdGrpcRequest 
                { 
                    Id = clotheId.ToString() 
                };
                QuestionsListGrpcResponse response = await client.GetAnswersAndQuestionByClotheIdAsync(request, cancellationToken: cancellationToken);
                logger.LogInformation("Successfully fetched Q&A for ClotheId: {ClotheId}", clotheId);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching Q&A for ClotheId: {ClotheId}", clotheId);
                throw;
            }
        }

        public async Task<ReviewStatisticGrpcResponse> GetStatisticsByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching statistics for ClotheId: {ClotheId}", clotheId);
                ReviewClotheIdGrpcRequest request = new ReviewClotheIdGrpcRequest {
                    Id = clotheId.ToString() 
                };
                ReviewStatisticGrpcResponse response = await client.GetStatisticsByClotheIdAsync(request, cancellationToken: cancellationToken);
                logger.LogInformation("Successfully fetched statistics for ClotheId: {ClotheId}", clotheId);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching statistics for ClotheId: {ClotheId}", clotheId);
                throw;
            }
        }

        public async Task<PendingReviewsCountGrpcResponse> GetPendingReviewsCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching pending reviews count...");
                PendingReviewsCountGrpcResponse response = await client.GetPendingReviewsCountAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);

                logger.LogInformation("Successfully fetched pending reviews count: {ReviewsCount}", response.ReviewsCount);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching pending reviews count");
                throw;
            }
        }

        public async Task<QuestionsWithoutAnswerGrpcResponse> GetQuestionsWithoutAnswerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching questions without answer count...");
                QuestionsWithoutAnswerGrpcResponse response = await client.GetQuestionsCountWithoutAnswerAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);

                logger.LogInformation("Successfully fetched questions without answer count: {QuestionsCount}", response.QuestionsCount);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching questions without answer count");
                throw;
            }
        }
    }
}
