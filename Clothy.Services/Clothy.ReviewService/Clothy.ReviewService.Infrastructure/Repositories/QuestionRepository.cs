using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces.Repositories;
using Clothy.ReviewService.Infrastructure.DB;
using MongoDB.Driver;
using Clothy.ReviewService.Domain.Exceptions;
using Clothy.Shared.Helpers;

namespace Clothy.ReviewService.Infrastructure.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(MongoDbContext context, IClientSessionHandle? session = null) : base(context, session)
        {

        }

        public async Task<PagedList<Question>> GetQuestionsAsync(QuestionQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Question>.Filter;
            var filter = filterBuilder.Empty;

            if (queryParameters.UserId.HasValue) filter &= filterBuilder.Eq(q => q.User.UserId, queryParameters.UserId.Value);

            if (queryParameters.ClotheItemId.HasValue) filter &= filterBuilder.Eq(q => q.ClotheItemId, queryParameters.ClotheItemId.Value);

            IFindFluent<Question, Question> findFluent = collection.Find(filter);

            if (!string.IsNullOrWhiteSpace(queryParameters.OrderBy))
            {
                var sortHelper = new MongoSortHelper<Question>();
                findFluent = findFluent.Sort(sortHelper.ApplySort(queryParameters.OrderBy));
            }

            return await PagedList<Question>.ToPagedListAsync(findFluent, queryParameters.PageNumber, queryParameters.PageSize, cancellationToken);
        }

        public async Task AddAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default)
        {
            if (answer == null) throw new EmptyValueException("Answer");

            var update = Builders<Question>.Update.Push(q => q.Answers, answer)
                                                .CurrentDate(q => q.UpdatedAt);

            await collection.UpdateOneAsync(q => q.Id == questionId, update, cancellationToken: cancellationToken);
        }

        public async Task UpdateAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default)
        {
            if (answer == null) throw new EmptyValueException("Answer");

            var filter = Builders<Question>.Filter.And(
                Builders<Question>.Filter.Eq(q => q.Id, questionId),
                Builders<Question>.Filter.ElemMatch(q => q.Answers, a => a.Id == answer.Id)
            );

            var update = Builders<Question>.Update
                .Set(q => q.Answers[-1], answer) 
                .CurrentDate(q => q.UpdatedAt);

            await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task UpdateQuestionAsync(Question question, CancellationToken cancellationToken = default)
        {
            if (question == null) throw new EmptyValueException("Question");

            var update = Builders<Question>.Update
                .Set(q => q.QuestionText, question.QuestionText)
                .CurrentDate(q => q.UpdatedAt);

            await collection.UpdateOneAsync(q => q.Id == question.Id, update, cancellationToken: cancellationToken);
        }

        public async Task DeleteAnswerAsync(string questionId, string answerId, CancellationToken cancellationToken = default)
        {
            var update = Builders<Question>.Update.PullFilter(q => q.Answers, a => a.Id == answerId)
                                                .CurrentDate(q => q.UpdatedAt);

            await collection.UpdateOneAsync(q => q.Id == questionId, update, cancellationToken: cancellationToken);
        }
    }
}
