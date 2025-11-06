using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Exceptions;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces.Repositories;
using Clothy.ReviewService.Domain.Interfaces.Services;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Clothy.ReviewService.Application.Services
{
    public class QuestionService : IQuestionService
    {
        private IQuestionRepository questionRepository;
        private IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient;

        public QuestionService(IQuestionRepository questionRepository, IClotheItemIdValidatorGrpcClient clotheItemIdValidatorGrpcClient)
        {
            this.questionRepository = questionRepository;
            this.clotheItemIdValidatorGrpcClient = clotheItemIdValidatorGrpcClient;
        }

        public async Task<PagedList<Question>> GetQuestionsAsync(QuestionQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            return await questionRepository.GetQuestionsAsync(queryParameters, cancellationToken);
        }

        public async Task<Question?> GetQuestionByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(id, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {id} not found!");
            
            return question;
        }

        public async Task<Question> AddQuestionAsync(Question question, CancellationToken cancellationToken = default)
        {
            ClotheItemIdToValidate clotheItemIdToValidate = new ClotheItemIdToValidate();
            clotheItemIdToValidate.ClotheId = question.ClotheItemId.ToString();
            ClotheItemResponse clotheItemResponse = await clotheItemIdValidatorGrpcClient.ValidateClotheItemIdAsync(clotheItemIdToValidate);

            if (!clotheItemResponse.IsValid) throw new ValidationFailedException($"Clothe item ID validation failed: {clotheItemResponse.ErrorMessage}");

            if (question == null) throw new EmptyValueException("Question cannot be null!");
            await questionRepository.AddAsync(question, cancellationToken);
            
            return question;
        }

        public async Task AddAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(questionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {questionId} not found!");
            
            await questionRepository.AddAnswerAsync(questionId, answer, cancellationToken);
        }

        public async Task UpdateAnswerAsync(string questionId, string answerId, string newText, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(questionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {questionId} not found!");

            Answer? answer = question.Answers.FirstOrDefault(a => a.Id == answerId);
            if (answer == null) throw new NotFoundException($"Answer with ID {answerId} not found!");

            answer.UpdateAnswer(newText);
            await questionRepository.UpdateAnswerAsync(questionId, answer, cancellationToken);
        }

        public async Task UpdateQuestionAsync(string questionId, string newText, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(questionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {questionId} not found!");

            question.UpdateQuestion(newText);
            await questionRepository.UpdateQuestionAsync(question, cancellationToken);
        }

        public async Task DeleteAnswerAsync(string questionId, string answerId, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(questionId, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {questionId} not found!");
            
            await questionRepository.DeleteAnswerAsync(questionId, answerId, cancellationToken);
        }

        public async Task DeleteQuestionAsync(string id, CancellationToken cancellationToken = default)
        {
            Question? question = await questionRepository.GetByIdAsync(id, cancellationToken);
            if (question == null) throw new NotFoundException($"Question with ID {id} not found!");
            
            await questionRepository.DeleteAsync(id, cancellationToken);
        }
    }
}