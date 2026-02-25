using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Infrastructure.DB;
using MongoDB.Driver;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Events.UserEvents;

namespace Clothy.ReviewService.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(MongoDbContext context, IClientSessionHandle? session = null) : base(context, session)
        {

        }

        public async Task<ReviewStatistics> GetReviewStatisticsAsync(Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Review>.Filter.Eq(review => review.ClotheInfo.ClotheItemId, clotheItemId) & 
                Builders<Review>.Filter.Eq(r => r.Status, ReviewStatus.Confirmed);

            var reviews = await collection.Find(filter).ToListAsync(cancellationToken);

            if (reviews.Count == 0)
            {
                return new ReviewStatistics
                {
                    ClotheItemId = clotheItemId,
                    TotalReviews = 0,
                    AverageRating = 0,
                    FiveStars = 0,
                    FourStars = 0,
                    ThreeStars = 0,
                    TwoStars = 0,
                    OneStar = 0
                };
            }

            return new ReviewStatistics
            {
                ClotheItemId = clotheItemId,
                TotalReviews = reviews.Count,
                FiveStars = reviews.Count(review => review.Rating == 5),
                FourStars = reviews.Count(review => review.Rating == 4),
                ThreeStars = reviews.Count(review => review.Rating == 3),
                TwoStars = reviews.Count(review => review.Rating == 2),
                OneStar = reviews.Count(review => review.Rating == 1),
                AverageRating = reviews.Average(review => review.Rating)
            };
        }

        public async Task<PagedList<Review>> GetReviewsAsync(ReviewQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Review>.Filter;
            var filter = filterBuilder.Empty;

            if (queryParameters.UserId.HasValue) filter &= filterBuilder.Eq(review => review.User.UserId, queryParameters.UserId.Value);

            if (queryParameters.ClotheItemId.HasValue)
            {
                filter &= filterBuilder.Eq(review => review.ClotheInfo.ClotheItemId, queryParameters.ClotheItemId.Value);

                if (!queryParameters.Status.HasValue)
                {
                    filter &= filterBuilder.Eq(review => review.Status, ReviewStatus.Confirmed);
                }
            }

            if (queryParameters.Status.HasValue) filter &= filterBuilder.Eq(review => review.Status, queryParameters.Status.Value);

            if (queryParameters.Rating.HasValue) filter &= filterBuilder.Eq(review => review.Rating, queryParameters.Rating.Value);

            IFindFluent<Review, Review> findFluent = collection.Find(filter);

            if (!string.IsNullOrWhiteSpace(queryParameters.OrderBy))
            {
                MongoSortHelper<Review> sortHelper = new MongoSortHelper<Review>();
                findFluent = findFluent.Sort(sortHelper.ApplySort(queryParameters.OrderBy));
            }

            return await PagedList<Review>.ToPagedListAsync(findFluent, queryParameters.PageNumber, queryParameters.PageSize, cancellationToken);
        }

        public async Task<bool> HasUserReviewedClotheAsync(Guid userId, Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Review>.Filter;
            var filter = filterBuilder.Eq(review => review.User.UserId, userId) &
                         filterBuilder.Eq(review => review.ClotheInfo.ClotheItemId, clotheItemId);

            long count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task<bool> ClotheItemExistsAsync(Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Review>.Filter.Eq(review => review.ClotheInfo.ClotheItemId, clotheItemId);
            long count = await collection.CountDocumentsAsync(filterBuilder, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task DeleteAllReviewsByClotheId(Guid clotheId, CancellationToken cancellationToken = default)
        {
            var reviewsByClotheId = Builders<Review>.Filter.Eq(tempReview => tempReview.ClotheInfo.ClotheItemId, clotheId);

            await collection.DeleteManyAsync(reviewsByClotheId, cancellationToken);
        }

        public async Task DeleteAllReviewsByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            var reviewsByUserId = Builders<Review>.Filter.Eq(tempReview => tempReview.User.UserId, userId);

            await collection.DeleteManyAsync(reviewsByUserId, cancellationToken);
        }

        public async Task<int> GetPendingReviewsCountAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<Review>.Filter.Eq(review => review.Status, ReviewStatus.Pending);

            long count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return (int)count;
        }

        public async Task UpdateUserInfoInReviewsAsync(UserUpdatedEvent userUpdatedEvent, bool newPhoto = false, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.User.UserId, userUpdatedEvent.UserId);

            var update = Builders<Review>.Update
                .Set(review => review.User.FirstName, userUpdatedEvent.FirstName)
                .Set(review => review.User.LastName, userUpdatedEvent.LastName)
                .CurrentDate(date => date.UpdatedAt);

            if(newPhoto) update.Set(review => review.User.PhotoUrl, userUpdatedEvent.PhotoUrl);

            await collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}