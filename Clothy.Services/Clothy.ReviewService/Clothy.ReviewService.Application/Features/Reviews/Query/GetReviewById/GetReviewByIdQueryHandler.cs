using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewById
{
    public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Review?>
    {
        private IReviewService reviewService;

        public GetReviewByIdQueryHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<Review?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            return await reviewService.GetReviewByIdAsync(request.ReviewId, cancellationToken);
        }
    }
}
