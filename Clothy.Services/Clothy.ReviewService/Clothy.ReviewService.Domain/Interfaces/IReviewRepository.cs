using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Clothy.Shared.Events.UserEvents;

namespace Clothy.ReviewService.Domain.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task UpdateUserInfoInReviewsAsync(UserUpdatedEvent userUpdatedEvent, bool newPhoto = false, CancellationToken cancellationToken = default);
        Task DeleteAllReviewsByClotheId(Guid clotheId, CancellationToken cancellationToken = default);
        Task DeleteAllReviewsByUserId(Guid userId, CancellationToken cancellationToken = default);
        Task<ReviewStatistics> GetReviewStatisticsAsync(Guid clotheItemId, CancellationToken cancellationToken = default);
        Task<bool> HasUserReviewedClotheAsync(Guid userId, Guid clotheItemId, CancellationToken cancellationToken = default);
        Task<PagedList<Review>> GetReviewsAsync(ReviewQueryParameters queryParameters, CancellationToken cancellationToken = default);
        Task<bool> ClotheItemExistsAsync(Guid clotheItemId, CancellationToken cancellationToken = default);
    }
}
