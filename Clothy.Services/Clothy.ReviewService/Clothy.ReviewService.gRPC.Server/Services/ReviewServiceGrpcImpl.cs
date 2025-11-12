using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.gRPC.Server.Services
{
    public class ReviewServiceGrpcImpl : ReviewServiceGrpc.ReviewServiceGrpcBase
    {
        private IReviewRepository reviewRepository;
        private IQuestionRepository questionRepository;
        private ILogger<ReviewServiceGrpcImpl> logger;

        public ReviewServiceGrpcImpl(IReviewRepository reviewRepository, IQuestionRepository questionRepository, ILogger<ReviewServiceGrpcImpl> logger)
        {
            this.reviewRepository = reviewRepository;
            this.questionRepository = questionRepository;
            this.logger = logger;
        }

        public override async Task<ReviewsListGrpcResponse> GetReviewsByClotheId(ReviewClotheIdGrpcRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching reviews for ClotheItemId: {ClotheId}", request.Id);

            try
            {
                ReviewQueryParameters queryParameters = new ReviewQueryParameters
                {
                    ClotheItemId = Guid.Parse(request.Id),
                    PageNumber = 1,
                    PageSize = 10
                };

                PagedList<Review> pagedReviews = await reviewRepository.GetReviewsAsync(queryParameters, context.CancellationToken);

                logger.LogInformation("Successfully fetched {Count} reviews for ClotheItemId: {ClotheId}", pagedReviews.Items.Count, request.Id);

                ReviewsListGrpcResponse response = new ReviewsListGrpcResponse();
                response.Reviews.AddRange(pagedReviews.Items.Select(review => new ReviewGrpcResponse
                {
                    Id = review.Id.ToString(),
                    User = new UserGrpcResponse
                    {
                        Id = review.User.UserId.ToString(),
                        FirstName = review.User.FirstName,
                        LastName = review.User.LastName,
                        PhotoUrl = review.User.PhotoUrl
                    },
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt.ToString()
                }));

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching reviews for ClotheItemId: {ClotheId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during fetching reviews"));
            }
        }

        public override async Task<QuestionsListGrpcResponse> GetAnswersAndQuestionByClotheId(ReviewClotheIdGrpcRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching questions and answers for ClotheItemId: {ClotheId}", request.Id);

            try
            {
                QuestionQueryParameters queryParameters = new QuestionQueryParameters
                {
                    ClotheItemId = Guid.Parse(request.Id),
                    PageNumber = 1,
                    PageSize = 10
                };

                PagedList<Question> pagedQuestions = await questionRepository.GetQuestionsAsync(queryParameters, context.CancellationToken);

                logger.LogInformation("Successfully fetched {Count} questions for ClotheItemId: {ClotheId}", pagedQuestions.Items.Count, request.Id);

                QuestionsListGrpcResponse response = new QuestionsListGrpcResponse();
                response.Questions.AddRange(pagedQuestions.Items.Select(question => new QuestionGrpcResponse
                {
                    Id = question.Id.ToString(),
                    User = new UserGrpcResponse
                    {
                        Id = question.User.UserId.ToString(),
                        FirstName = question.User.FirstName,
                        LastName = question.User.LastName,
                        PhotoUrl = question.User.PhotoUrl
                    },
                    QuestionText = question.QuestionText,
                    Answers = { question.Answers.Select(answer => new AnswerGrpcResponse
                    {
                        Id = answer.Id.ToString(),
                        User = new UserGrpcResponse
                        {
                            Id = answer.User.UserId.ToString(),
                            FirstName = answer.User.FirstName,
                            LastName = answer.User.LastName,
                            PhotoUrl = answer.User.PhotoUrl
                        },
                        AnswerText = answer.AnswerText
                    })}
                }));

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching questions and answers for ClotheItemId: {ClotheId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during fetching questions and answers"));
            }
        }

        public override async Task<ReviewStatisticGrpcResponse> GetStatisticsByClotheId(ReviewClotheIdGrpcRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching review statistics for ClotheItemId: {ClotheId}", request.Id);

            try
            {
                ReviewStatistics stats = await reviewRepository.GetReviewStatisticsAsync(Guid.Parse(request.Id), context.CancellationToken);

                logger.LogInformation("Successfully fetched statistics for ClotheItemId: {ClotheId}", request.Id);

                return new ReviewStatisticGrpcResponse
                {
                    ClotheItemId = request.Id,
                    TotalReviews = stats.TotalReviews,
                    FiveStars = stats.FiveStars,
                    FourStars = stats.FourStars,
                    ThreeStars = stats.ThreeStars,
                    TwoStars = stats.TwoStars,
                    OneStars = stats.OneStar,
                    AverageRating = stats.AverageRating
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching review statistics for ClotheItemId: {ClotheId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during fetching statistics"));
            }
        }
    }
}