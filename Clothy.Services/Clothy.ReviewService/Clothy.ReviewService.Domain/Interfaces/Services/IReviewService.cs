using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.Domain.Interfaces.Services
{
    public interface IReviewService
    {
        Task<PagedList<Review>> GetReviewsAsync(ReviewQueryParameters queryParameters, CancellationToken cancellationToken = default);
        Task<Review?> GetReviewByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Review> AddReviewAsync(Review review, CancellationToken cancellationToken = default);
        Task UpdateReviewAsync(string id, string newComment, int newRating, CancellationToken cancellationToken = default);
        Task DeleteReviewAsync(string id, CancellationToken cancellationToken = default);
    }
}
