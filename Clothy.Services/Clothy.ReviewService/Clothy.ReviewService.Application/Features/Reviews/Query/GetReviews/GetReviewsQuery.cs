using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviews
{
    public class GetReviewsQuery : IRequest<PagedList<Review>>
    {
        public ReviewQueryParameters QueryParameters { get; }

        public GetReviewsQuery(ReviewQueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}
