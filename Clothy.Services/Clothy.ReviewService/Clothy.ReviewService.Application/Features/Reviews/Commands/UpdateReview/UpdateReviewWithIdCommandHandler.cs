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

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview
{
    public class UpdateReviewWithIdCommandHandler : ICommandHandler<UpdateReviewWithIdCommand>
    {
        private IReviewRepository reviewRepository;

        public UpdateReviewWithIdCommandHandler(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<Unit> Handle(UpdateReviewWithIdCommand request, CancellationToken cancellationToken)
        {
            Review? review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {request.ReviewId} not found!");

            review.UpdateComment(request.Comment, request.Rating);
            await reviewRepository.UpdateAsync(review, cancellationToken);

            return Unit.Value;
        }
    }
}