using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : ICommandHandler<DeleteReviewCommand>
    {
        private IReviewRepository reviewRepository;

        public DeleteReviewCommandHandler(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            Review? review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {request.ReviewId} not found!");

            await reviewRepository.DeleteAsync(request.ReviewId, cancellationToken);
            return Unit.Value;
        }
    }
}
