using Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Events.UserEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Application.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
    {
        private ILogger<ReviewsAndQuestionsDeletionConsumer> logger;
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;
        private IEventLogService eventLogService;

        public UserUpdatedConsumer(
            ILogger<ReviewsAndQuestionsDeletionConsumer> logger, 
            IReviewRepository reviewRepository, 
            IQuestionRepository questionRepository, 
            IEventLogService eventLogService)
        {
            this.logger = logger;
            this.reviewRepository = reviewRepository;
            this.questionRepository = questionRepository;
            this.eventLogService = eventLogService;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            Guid eventId = context.Message.EventId;
            if (await eventLogService.HasEventProcessedAsync(eventId, context.CancellationToken))
            {
                logger.LogWarning("Duplicate UserUpdatedEvent detected: {EventId}", eventId);
                return;
            }

            UserUpdatedEvent userUpdatedEvent = context.Message;
            logger.LogInformation("ReviewService: Received UserUpdatedEvent for UserId: {UserId}", userUpdatedEvent.UserId);

            bool newPhoto = userUpdatedEvent.PhotoUrl != null;

            await reviewRepository.UpdateUserInfoInReviewsAsync(userUpdatedEvent, newPhoto, context.CancellationToken);
            await questionRepository.UpdateUserInfoAsync(userUpdatedEvent, newPhoto, context.CancellationToken);

            await eventLogService.MarkEventAsProcessedAsync(eventId, context.CancellationToken);
            logger.LogInformation("ReviewService: Updated all reviews and questions for UserId: {UserId}", userUpdatedEvent.UserId);
        }
    }
}
