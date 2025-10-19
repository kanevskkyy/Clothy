using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview
{
    public class UpdateReviewCommandHandler : ICommandHandler<UpdateReviewCommand>
    {
        private IReviewService reviewService;

        public UpdateReviewCommandHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<Unit> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            await reviewService.UpdateReviewAsync(request.ReviewId.ToString(), request.Comment, request.Rating, cancellationToken);
            return Unit.Value;
        }
    }
}
