using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces.Repositories;
using Clothy.ReviewService.Domain.Interfaces.Services;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.Domain.Exceptions;

namespace Clothy.ReviewService.Application.Services
{
    public class ReviewService : IReviewService
    {
        private IReviewRepository reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<ReviewStatistics> GetReviewStatisticsAsync(Guid clotheItemId, CancellationToken cancellationToken = default)
        {
            if(!await reviewRepository.ClotheItemExistsAsync(clotheItemId, cancellationToken)) throw new NotFoundException($"ClotheItem  with ID {clotheItemId} not found!");

            return await reviewRepository.GetReviewStatisticsAsync(clotheItemId, cancellationToken);
        }

        public async Task<PagedList<Review>> GetReviewsAsync(ReviewQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            return await reviewRepository.GetReviewsAsync(queryParameters, cancellationToken);
        }

        public async Task<Review?> GetReviewByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Review? review = await reviewRepository.GetByIdAsync(id, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {id} not found!");
            
            return review;
        }

        public async Task<Review> AddReviewAsync(Review review, CancellationToken cancellationToken = default)
        {
            bool alreadyExists = await reviewRepository.HasUserReviewedClotheAsync(review.User.UserId, review.ClotheItemId, cancellationToken);
            if (alreadyExists) throw new AlreadyExistsException("User has already reviewed this clothe!");

            await reviewRepository.AddAsync(review, cancellationToken);
            return review;
        }

        public async Task UpdateReviewAsync(string id, string newComment, int newRating, CancellationToken cancellationToken = default)
        {
            Review? review = await reviewRepository.GetByIdAsync(id, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {id} not found!");

            review.UpdateComment(newComment, newRating);
            await reviewRepository.UpdateAsync(review, cancellationToken);
        }

        public async Task DeleteReviewAsync(string id, CancellationToken cancellationToken = default)
        {
            Review? review = await reviewRepository.GetByIdAsync(id, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {id} not found!");

            await reviewRepository.DeleteAsync(id, cancellationToken);
        }
    }
}
