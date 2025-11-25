using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Events.UserEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private ILogger<UserDeletedConsumer> logger;
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;
        private IEventLogService eventLogService;

        public UserDeletedConsumer(
            ILogger<UserDeletedConsumer> logger, 
            IReviewRepository reviewRepository, 
            IQuestionRepository questionRepository, 
            IEventLogService eventLogService)
        {
            this.logger = logger;
            this.reviewRepository = reviewRepository;
            this.questionRepository = questionRepository;
            this.eventLogService = eventLogService;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            Guid eventId = context.Message.EventId;
            if (await eventLogService.HasEventProcessedAsync(eventId, context.CancellationToken))
            {
                logger.LogWarning("Duplicate UserDeletedEvent detected: {EventId}", eventId);
                return;
            }

            UserDeletedEvent userDeletedEvent = context.Message;

            logger.LogInformation("ReviewService: Received UserDeletedEvent for UserID: {UserId}", userDeletedEvent.UserId);

            await questionRepository.DeleteAllQuestionByUserIdAsync(userDeletedEvent.UserId, context.CancellationToken);
            await reviewRepository.DeleteAllReviewsByUserId(userDeletedEvent.UserId, context.CancellationToken);

            await eventLogService.MarkEventAsProcessedAsync(eventId, context.CancellationToken);

            logger.LogInformation("ReviewService: Deleted all reviews, questions and answers for UserId: {UserId}", userDeletedEvent.UserId);
        }
    }
}
