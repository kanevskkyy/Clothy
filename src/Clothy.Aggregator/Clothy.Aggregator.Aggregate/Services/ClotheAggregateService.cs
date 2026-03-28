using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<ClotheDetailFullDTO> GetFullClotheDetailAsync(string slug, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting aggregation for slug: {Slug}", slug);

            ClotheDetailGrpcResponse clotheInfo = await clotheGrpcClient.GetClotheByIdAsync(slug, cancellationToken);

            var reviewsTask = reviewGrpcClient.GetReviewsByClotheIdAsync(Guid.Parse(clotheInfo.Id), cancellationToken);
            var questionsTask = reviewGrpcClient.GetQuestionsAndAnswersByClotheIdAsync(Guid.Parse(clotheInfo.Id), cancellationToken);
            var statsTask = reviewGrpcClient.GetStatisticsByClotheIdAsync(Guid.Parse(clotheInfo.Id), cancellationToken);

            await Task.WhenAll(reviewsTask, questionsTask, statsTask);

            ReviewsListGrpcResponse reviews = reviewsTask.Result;
            QuestionsListGrpcResponse questions = questionsTask.Result;
            ReviewStatisticGrpcResponse stats = statsTask.Result;

            ClotheDetailFullDTO clotheDetailFullDTO = new ClotheDetailFullDTO
            {
                ClotheDetailDTO = clotheInfo,
                Reviews = new PagedList<ReviewGrpcResponse>
                {
                    CurrentPage = reviews.CurrentPage,
                    TotalPages = reviews.TotalPages,
                    PageSize = reviews.PageSize,
                    TotalCount = reviews.TotalCount,
                    Items = reviews.Items.ToList()
                },
                Questions = new PagedList<QuestionGrpcResponse>
                {
                    CurrentPage = questions.CurrentPage,
                    TotalPages = questions.TotalPages,
                    PageSize = questions.PageSize,
                    TotalCount = questions.TotalCount,
                    Items = questions.Items.ToList()
                },
                Statistics = stats
            };

            logger.LogInformation("Successfully aggregated full detail for ClotheId: {ClotheId}", clotheInfo.Id);
            return clotheDetailFullDTO;
        }
    }
}
