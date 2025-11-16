using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events.ClotheItemEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions
{
    public class ReviewsAndQuestionsDeletionConsumer : IConsumer<ClotheItemDeletedEvent>
    {
        private ILogger<ReviewsAndQuestionsDeletionConsumer> logger;
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;

        public ReviewsAndQuestionsDeletionConsumer(IReviewRepository reviewRepository, IQuestionRepository questionRepository, ILogger<ReviewsAndQuestionsDeletionConsumer> logger)
        {
            this.reviewRepository = reviewRepository;
            this.questionRepository = questionRepository;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<ClotheItemDeletedEvent> context)
        {
            var message = context.Message;

            logger.LogInformation("ReviewService: Received ClotheItemDeletedEvent for ClotheId: {ClotheId}", message.ClotheId);

            await reviewRepository.DeleteAllReviewsByClotheId(message.ClotheId, context.CancellationToken);
            await questionRepository.DeleteAllQuestionsByClotheId(message.ClotheId, context.CancellationToken);

            logger.LogInformation("ReviewService: Deleted all reviews and questions for ClotheId: {ClotheId}", message.ClotheId);
        }
    }
}
