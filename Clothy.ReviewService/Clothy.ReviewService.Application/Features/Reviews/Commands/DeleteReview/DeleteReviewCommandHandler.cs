using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : ICommandHandler<DeleteReviewCommand>
    {
        private IReviewService reviewService;

        public DeleteReviewCommandHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            await reviewService.DeleteReviewAsync(request.ReviewId.ToString(), cancellationToken);
            return Unit.Value;
        }
    }
}
