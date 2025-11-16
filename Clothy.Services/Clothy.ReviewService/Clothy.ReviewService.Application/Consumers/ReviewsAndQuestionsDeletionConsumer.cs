using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions
{
    public class ReviewsAndQuestionsDeletionConsumer : IConsumer<ClotheItemDeletedEvent>
    {
        private ILogger<ReviewsAndQuestionsDeletionConsumer> logger;
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;
        private IEventLogService eventLogService;

        public ReviewsAndQuestionsDeletionConsumer(IReviewRepository reviewRepository, IQuestionRepository questionRepository, ILogger<ReviewsAndQuestionsDeletionConsumer> logger, IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
            this.reviewRepository = reviewRepository;
            this.questionRepository = questionRepository;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<ClotheItemDeletedEvent> context)
        {
            Guid eventId = context.Message.EventId;
            if (await eventLogService.HasEventProcessedAsync(eventId, context.CancellationToken))
            {
                logger.LogWarning("Duplicate ClotheItemDeletedEvent detected: {EventId}", eventId);
                return;
            }

            ClotheItemDeletedEvent clotheItemDeletedEvent = context.Message;

            logger.LogInformation("ReviewService: Received ClotheItemDeletedEvent for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);

            await reviewRepository.DeleteAllReviewsByClotheId(clotheItemDeletedEvent.ClotheId, context.CancellationToken);
            await questionRepository.DeleteAllQuestionsByClotheId(clotheItemDeletedEvent.ClotheId, context.CancellationToken);

            await eventLogService.MarkEventAsProcessedAsync(eventId, context.CancellationToken);

            logger.LogInformation("ReviewService: Deleted all reviews and questions for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);
        }
    }
}
