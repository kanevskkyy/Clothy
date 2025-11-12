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

namespace Clothy.ReviewService.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(MongoDbContext context, IClientSessionHandle? session = null) : base(context, session)
        {

        }

        public async Task<ReviewStatistics> GetReviewStatisticsAsync(Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.ClotheItemId, clotheItemId);
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
                FiveStars = reviews.Count(r => r.Rating == 5),
                FourStars = reviews.Count(r => r.Rating == 4),
                ThreeStars = reviews.Count(r => r.Rating == 3),
                TwoStars = reviews.Count(r => r.Rating == 2),
                OneStar = reviews.Count(r => r.Rating == 1),
                AverageRating = reviews.Average(r => r.Rating)
            };
        }

        public async Task<PagedList<Review>> GetReviewsAsync(ReviewQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Review>.Filter;
            var filter = filterBuilder.Empty;

            if (queryParameters.UserId.HasValue) filter &= filterBuilder.Eq(r => r.User.UserId, queryParameters.UserId.Value);

            if (queryParameters.ClotheItemId.HasValue) filter &= filterBuilder.Eq(r => r.ClotheItemId, queryParameters.ClotheItemId.Value);

            if (queryParameters.Rating.HasValue) filter &= filterBuilder.Eq(r => r.Rating, queryParameters.Rating.Value);

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
            var filter = filterBuilder.Eq(r => r.User.UserId, userId) &
                         filterBuilder.Eq(r => r.ClotheItemId, clotheItemId);

            long count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task<bool> ClotheItemExistsAsync(Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Review>.Filter.Eq(p => p.ClotheItemId, clotheItemId);
            long count = await collection.CountDocumentsAsync(filterBuilder, cancellationToken: cancellationToken);
            return count > 0;
        }
    }
}
