using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions
{
    public class ReviewsAndQuestionListenerEvent : BaseEventListenerService<ClotheItemDeletedEvent>
    {
        protected override string ExchangeName => "clothe-item-deleted";
        protected override string QueueName => "delete-clothe-item-reviews-queue";
        protected override string RoutingKey => "clothe-item-deleted-reviews-key";

        public ReviewsAndQuestionListenerEvent(IConnectionFactory connectionFactory, ILogger<ReviewsAndQuestionListenerEvent> logger, IServiceScopeFactory serviceScopeFactory) : base(connectionFactory, logger, serviceScopeFactory)
        {

        }
    }
}
