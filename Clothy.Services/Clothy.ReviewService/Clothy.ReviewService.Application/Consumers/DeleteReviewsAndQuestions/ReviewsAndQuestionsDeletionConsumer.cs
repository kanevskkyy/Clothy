using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions
{
    public class ReviewsAndQuestionsDeletionConsumer : IEventHandler<ClotheItemDeletedEvent>
    {
        private ILogger<ReviewsAndQuestionsDeletionConsumer> logger;
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;

        public ReviewsAndQuestionsDeletionConsumer(IReviewRepository reviewRepository, IQuestionRepository questionRepository, ILogger<ReviewsAndQuestionsDeletionConsumer> logger)
        {
            this.reviewRepository = reviewRepository;
            this.logger = logger;
            this.questionRepository = questionRepository;
        }

        public async Task HandleAsync(ClotheItemDeletedEvent clotheItemDeletedEvent, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("ReviewService: Received ClotheItemDeletedEvent for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);

            await reviewRepository.DeleteAllReviewsByClotheId(clotheItemDeletedEvent.ClotheId, cancellationToken);
            await questionRepository.DeleteAllQuestionsByClotheId(clotheItemDeletedEvent.ClotheId, cancellationToken);

            logger.LogInformation("ReviewService: delete all reviews and question for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);
        }
    }
}
