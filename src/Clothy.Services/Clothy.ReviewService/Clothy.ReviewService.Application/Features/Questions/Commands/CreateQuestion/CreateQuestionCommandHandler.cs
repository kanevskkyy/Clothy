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

namespace Clothy.ReviewService.Application.Features.Questions.Commands.CreateQuestion
{
    public class CreateQuestionCommandHandler : ICommandHandler<CreateQuestionCommand, Question>
    {
        private IQuestionRepository questionRepository;
        private IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient;
        private Counter<long> questionsCreated;

        public CreateQuestionCommandHandler(IQuestionRepository questionRepository, IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient, Meter meter)
        {
            this.clotheItemIdValidatorGrpcClient = clotheItemIdValidatorGrpcClient;
            this.questionRepository = questionRepository;
            questionsCreated = meter.CreateCounter<long>(
                "clothy.reviewservice.questions-created",
                "count",
                "Total number of questions created");
        }

        public async Task<Question> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
        {
            UserInfo userInfo = new UserInfo(request.UserId, request.FirstName, request.LastName, request.PhotoUrl);

            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = request.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            ClotheInfo clotheInfo = new ClotheInfo(request.ClotheItemId, clotheItemResponse.ClotheName, clotheItemResponse.ClothePhotoUrl);
            Question question = new Question(clotheInfo, userInfo, request.QuestionText);

            await questionRepository.AddAsync(question, cancellationToken);
            questionsCreated.Add(1, new KeyValuePair<string, object?>("ClotheItemId", question.ClotheInfo.ClotheItemId));

            return question;
        }
    }
}