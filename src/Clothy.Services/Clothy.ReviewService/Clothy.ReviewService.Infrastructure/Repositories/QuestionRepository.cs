using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Helpers;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Infrastructure.DB;
using Clothy.Shared.Events.UserEvents;
using Clothy.Shared.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (queryParameters.UserId.HasValue) filter &= filterBuilder.Eq(question => question.User.UserId, queryParameters.UserId.Value);

            if (queryParameters.ClotheItemId.HasValue) filter &= filterBuilder.Eq(question => question.ClotheInfo.ClotheItemId, queryParameters.ClotheItemId.Value);

            if (queryParameters.WithoutAnswer == true)
            {
                filter &= filterBuilder.Or(
                    filterBuilder.Size(q => q.Answers, 0),
                    filterBuilder.Eq(q => q.Answers, null)
                );
            }

            IFindFluent<Question, Question> findFluent = collection.Find(filter);

            if (!string.IsNullOrWhiteSpace(queryParameters.OrderBy))
            {
                MongoSortHelper<Question> sortHelper = new MongoSortHelper<Question>();
                findFluent = findFluent.Sort(sortHelper.ApplySort(queryParameters.OrderBy));
            }

            return await PagedList<Question>.ToPagedListAsync(findFluent, queryParameters.PageNumber, queryParameters.PageSize, cancellationToken);
        }

        public async Task AddAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default)
        {
            var update = Builders<Question>.Update.Push(question => question.Answers, answer)
                                                .CurrentDate(question => question.UpdatedAt);

            await collection.UpdateOneAsync(question => question.Id == questionId, update, cancellationToken: cancellationToken);
        }

        public async Task UpdateAnswerAsync(string questionId, Answer answer, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Question>.Filter.And(
                Builders<Question>.Filter.Eq(question => question.Id, questionId),
                Builders<Question>.Filter.ElemMatch(question => question.Answers, a => a.Id == answer.Id)
            );

            var update = Builders<Question>.Update
                .Set(q => q.Answers[-1], answer) 
                .CurrentDate(question => question.UpdatedAt);

            await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task UpdateQuestionAsync(Question question, CancellationToken cancellationToken = default)
        {
            var update = Builders<Question>.Update
                .Set(tempQuestion => tempQuestion.QuestionText, question.QuestionText)
                .CurrentDate(question => question.UpdatedAt);

            await collection.UpdateOneAsync(q => q.Id == question.Id, update, cancellationToken: cancellationToken);
        }

        public async Task DeleteAnswerAsync(string questionId, string answerId, CancellationToken cancellationToken = default)
        {
            var update = Builders<Question>.Update.PullFilter(question => question.Answers, answer => answer.Id == answerId)
                                                .CurrentDate(question => question.UpdatedAt);

            await collection.UpdateOneAsync(question => question.Id == questionId, update, cancellationToken: cancellationToken);
        }

        public async Task DeleteAllQuestionsByClotheId(Guid clotheId, CancellationToken cancellationToken = default)
        {
            var questionsByClotheId = Builders<Question>.Filter.Eq(tempQuestion => tempQuestion.ClotheInfo.ClotheItemId, clotheId);

            await collection.DeleteManyAsync(questionsByClotheId, cancellationToken);
        }

        public async Task DeleteAllQuestionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var questionsByUserId = Builders<Question>.Filter.Eq(tempQuestion => tempQuestion.User.UserId, userId);
            
            await collection.DeleteManyAsync(questionsByUserId, cancellationToken);

            var filter = Builders<Question>.Filter.ElemMatch(
                q => q.Answers,
                a => a.User.UserId == userId
            );

            var update = Builders<Question>.Update.PullFilter(
                q => q.Answers,
                a => a.User.UserId == userId
            );

            await collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task UpdateUserInfoAsync(UserUpdatedEvent userUpdatedEvent, bool newPhoto = false, CancellationToken cancellationToken = default)
        {
            var questionByUserId = Builders<Question>.Filter.Eq(temp => temp.User.UserId, userUpdatedEvent.UserId);
            var questionUpdate = Builders<Question>.Update
                .Set(user => user.User.FirstName, userUpdatedEvent.FirstName)
                .Set(user => user.User.LastName, userUpdatedEvent.LastName)
                .CurrentDate(date => date.UpdatedAt);

            if (newPhoto) questionUpdate = questionUpdate.Set(user => user.User.PhotoUrl, userUpdatedEvent.PhotoUrl);

            await collection.UpdateManyAsync(questionByUserId, questionUpdate, cancellationToken: cancellationToken);

            var answerByUserId = Builders<Question>.Filter.ElemMatch(
                q => q.Answers,
                a => a.User.UserId == userUpdatedEvent.UserId
            );

            var answerUpdate = Builders<Question>.Update
                .Set("Answers.$[elem].User.FirstName", userUpdatedEvent.FirstName)
                .Set("Answers.$[elem].User.LastName", userUpdatedEvent.LastName)
                .CurrentDate(date => date.UpdatedAt);

            if (newPhoto)
                answerUpdate = answerUpdate.Set("Answers.$[elem].User.PhotoUrl", userUpdatedEvent.PhotoUrl);

            List<ArrayFilterDefinition> arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument { 
                        { 
                            "elem.User.UserId", BsonValue.Create(userUpdatedEvent.UserId) 
                        } 
                    }
                )
            };

            UpdateOptions updateOptions = new UpdateOptions { 
                ArrayFilters = arrayFilters 
            };

            await collection.UpdateManyAsync(answerByUserId, answerUpdate, updateOptions, cancellationToken);
        }
    }
}
