using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.Shared.Helpers;

namespace Clothy.ReviewService.Domain.Interfaces.Repositories
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task UpdateQuestionAsync(Question question, CancellationToken cancellationToken = default);
        Task<PagedList<Question>> GetQuestionsAsync(QuestionQueryParameters queryParameters, CancellationToken cancellationToken = default);
        Task AddAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default);
        Task UpdateAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default);
        Task DeleteAnswerAsync(string questionId, string answerId, CancellationToken cancellationToken = default);
    }
}
