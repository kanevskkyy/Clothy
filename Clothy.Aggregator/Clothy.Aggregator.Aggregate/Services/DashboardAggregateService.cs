using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Services
{
    public class DashboardAggregateService : IDashboardAggregateService
    {
        private IOrderGrpcClient orderGrpcClient;
        private IStockGrpcClient stockGrpcClient;
        private IReviewGrpcClient reviewGrpcClient;
        private ILogger<DashboardAggregateService> logger;

        public DashboardAggregateService(
            IOrderGrpcClient orderGrpcClient,
            IStockGrpcClient stockGrpcClient,
            IReviewGrpcClient reviewGrpcClient,
            ILogger<DashboardAggregateService> logger)
        {
            this.orderGrpcClient = orderGrpcClient;
            this.stockGrpcClient = stockGrpcClient;
            this.reviewGrpcClient = reviewGrpcClient;
            this.logger = logger;
        }

        public async Task<DashboardFullDTO> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching dashboard statistics...");
            try
            {
                var orderStatsTask = orderGrpcClient.GetOrderStatsAsync(cancellationToken);
                var stockTask = stockGrpcClient.GetTotalQuantityAsync(cancellationToken);
                var pendingReviewsTask = reviewGrpcClient.GetPendingReviewsCountAsync(cancellationToken);
                var questionsTask = reviewGrpcClient.GetQuestionsWithoutAnswerAsync(cancellationToken);

                await Task.WhenAll(orderStatsTask, stockTask, pendingReviewsTask, questionsTask);

                OrderStats orderStats = await orderStatsTask;
                GetTotalQuantityResponse stock = await stockTask;
                PendingReviewsCountGrpcResponse pendingReviews = await pendingReviewsTask;
                QuestionsWithoutAnswerGrpcResponse questions = await questionsTask;

                logger.LogInformation("Successfully fetched dashboard statistics");

                return new DashboardFullDTO
                {
                    NewOrdersCount = orderStats.NewOrdersCount,
                    TotalPrice = decimal.Parse(orderStats.TotalPrice, System.Globalization.CultureInfo.InvariantCulture),
                    PendingOrdersCount = orderStats.PendingOrders,
                    TotalItemsCount = stock.TotalQuantity,
                    PendingReviewCount = pendingReviews.ReviewsCount,
                    QuestionsWithoutAnswerCount = questions.QuestionsCount
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching dashboard statistics");
                throw;
            }
        }
    }
}