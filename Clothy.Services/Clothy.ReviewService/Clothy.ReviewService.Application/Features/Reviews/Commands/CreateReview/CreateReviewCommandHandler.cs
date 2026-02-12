using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, Review>
    {
        private IReviewRepository reviewRepository;
        private IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient;
        private ICheckUserPurchasedClotheGrpcClient checkUserPurchasedClotheGrpcClient;
        private Counter<long> reviewsCreated;

        public CreateReviewCommandHandler(IReviewRepository reviewRepository, 
            IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient, 
            Meter meter, 
            ICheckUserPurchasedClotheGrpcClient checkUserPurchasedClotheGrpcClient)
        {
            this.reviewRepository = reviewRepository;
            this.clotheItemIdValidatorGrpcClient = clotheItemIdValidatorGrpcClient;
            reviewsCreated = meter.CreateCounter<long>(
                "clothy.reviewservice.reviews-created",
                "count",
                "Total numbers of reviews created");
            this.checkUserPurchasedClotheGrpcClient = checkUserPurchasedClotheGrpcClient;
        }

        public async Task<Review> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            UserInfo userInfo = new UserInfo(request.UserId, request.FirstName, request.LastName, request.PhotoUrl);

            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = request.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            CheckUserPurchasedRequest purchasedRequest = new CheckUserPurchasedRequest()
            { 
                UserId = request.UserId.ToString(), 
                ClotheId = request.ClotheItemId.ToString()
            };
            CheckUserPurchasedResponse userPurchasedResponse = await checkUserPurchasedClotheGrpcClient.CheckUserPurchasedAsync(purchasedRequest);
            if (!userPurchasedResponse.Purchased) throw new ForbiddenException($"You cannot rate clothes that you did not order!");

            Review review = new Review(new ClotheInfo(request.ClotheItemId, userPurchasedResponse.ClotheName, userPurchasedResponse.ClothePhotoURL), userInfo, request.Rating, request.Comment);

            bool alreadyExists = await reviewRepository.HasUserReviewedClotheAsync(review.User.UserId, review.ClotheInfo.ClotheItemId, cancellationToken);
            if (alreadyExists) throw new AlreadyExistsException("User has already reviewed this clothe!");

            await reviewRepository.AddAsync(review, cancellationToken);
            reviewsCreated.Add(1, new KeyValuePair<string, object?>("ClotheId", review.ClotheInfo.ClotheItemId));

            return review;
        }
    }
}
