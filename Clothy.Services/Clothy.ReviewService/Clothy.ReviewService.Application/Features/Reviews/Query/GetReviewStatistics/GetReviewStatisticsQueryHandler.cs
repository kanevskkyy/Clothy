using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces.Services;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewStatistics
{
    public class GetReviewStatisticsQueryHandler : IRequestHandler<GetReviewStatisticsQuery, ReviewStatistics>
    {
        private IReviewService reviewService;

        public GetReviewStatisticsQueryHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<ReviewStatistics> Handle(GetReviewStatisticsQuery request, CancellationToken cancellationToken)
        {
            return await reviewService.GetReviewStatisticsAsync(request.ClotheItemId, cancellationToken);
        }
    }
}
