using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.Domain.Interfaces.Services
{
    public interface IQuestionService
    {
        Task UpdateQuestionAsync(string questionId, TextValue newText, CancellationToken cancellationToken = default);
        Task<PagedList<Question>> GetQuestionsAsync(QuestionQueryParameters queryParameters, CancellationToken cancellationToken = default);
        Task<Question?> GetQuestionByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Question> AddQuestionAsync(Question question, CancellationToken cancellationToken = default);
        Task AddAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default);
        Task UpdateAnswerAsync(string questionId, string answerId, TextValue newText, CancellationToken cancellationToken = default);
        Task DeleteAnswerAsync(string questionId, string answerId, CancellationToken cancellationToken = default);
        Task DeleteQuestionAsync(string id, CancellationToken cancellationToken = default);
    }
}
