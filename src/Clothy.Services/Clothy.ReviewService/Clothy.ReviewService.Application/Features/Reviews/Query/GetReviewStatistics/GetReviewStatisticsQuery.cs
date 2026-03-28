using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewStatistics
{
    public class GetReviewStatisticsQuery : IRequest<ReviewStatistics>
    {
        public Guid ClotheItemId { get; }

        public GetReviewStatisticsQuery(Guid clotheItemId)
        {
            ClotheItemId = clotheItemId;
        }
    }
}
