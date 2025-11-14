using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, Review>
    {
        private IReviewRepository reviewRepository;
        private IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient;
        private Counter<long> reviewsCreated;

        public CreateReviewCommandHandler(IReviewRepository reviewRepository, IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient, Meter meter)
        {
            this.reviewRepository = reviewRepository;
            this.clotheItemIdValidatorGrpcClient = clotheItemIdValidatorGrpcClient;
            reviewsCreated = meter.CreateCounter<long>(
                "clothy.reviewservice.reviews-created",
                "count",
                "Total numbers of reviews created");
        }

        public async Task<Review> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            Review review = new Review(request.ClotheItemId, request.User, request.Rating, request.Comment);
            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = review.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            bool alreadyExists = await reviewRepository.HasUserReviewedClotheAsync(review.User.UserId, review.ClotheItemId, cancellationToken);
            if (alreadyExists) throw new AlreadyExistsException("User has already reviewed this clothe!");

            await reviewRepository.AddAsync(review, cancellationToken);
            reviewsCreated.Add(1, new KeyValuePair<string, object?>("ClotheId", review.ClotheItemId));

            return review;
        }
    }
}
