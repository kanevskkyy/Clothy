using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.gRPC.Client.Services;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewStatistics
{
    public class GetReviewStatisticsQueryHandler : IRequestHandler<GetReviewStatisticsQuery, ReviewStatistics>
    {
        private IReviewRepository reviewRepository;
        private IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient;

        public GetReviewStatisticsQueryHandler(IReviewRepository reviewRepository, IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient)
        {
            this.reviewRepository = reviewRepository;
            this.clotheItemIdValidatorGrpcClient = clotheItemIdValidatorGrpcClient;
        }

        public async Task<ReviewStatistics> Handle(GetReviewStatisticsQuery request, CancellationToken cancellationToken)
        {
            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = request.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            if (!await reviewRepository.ClotheItemExistsAsync(request.ClotheItemId, cancellationToken)) throw new NotFoundException($"ClotheItem with ID {request.ClotheItemId} not found!");

            return await reviewRepository.GetReviewStatisticsAsync(request.ClotheItemId, cancellationToken);
        }
    }
}
