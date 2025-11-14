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
            Question question = new Question(request.ClotheItemId, request.User, request.QuestionText);

            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = question.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            await questionRepository.AddAsync(question, cancellationToken);
            questionsCreated.Add(1, new KeyValuePair<string, object?>("ClotheItemId", question.ClotheItemId));

            return question;
        }
    }
}
