using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces.Services;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviews
{
    public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, PagedList<Review>>
    {
        private IReviewService reviewService;

        public GetReviewsQueryHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<PagedList<Review>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            return await reviewService.GetReviewsAsync(request.QueryParameters, cancellationToken);
        }
    }
}
