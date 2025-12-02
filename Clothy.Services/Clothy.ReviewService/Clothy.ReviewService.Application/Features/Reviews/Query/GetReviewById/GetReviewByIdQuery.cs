using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewById
{
    public class GetReviewByIdQuery : IRequest<Review?>
    {
        public string ReviewId { get; }
        public bool IsAdmin { get; }
        public bool IsManager { get; }

        public GetReviewByIdQuery(string reviewId, bool isAdmin = false, bool isManager = false)
        {
            ReviewId = reviewId;
            IsAdmin = isAdmin;
            IsManager = isManager;
        }
    }
}
