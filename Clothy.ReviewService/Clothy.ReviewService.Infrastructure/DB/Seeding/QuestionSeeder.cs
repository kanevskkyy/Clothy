using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.Seeding
{
    public class QuestionSeeder : IDataSeeder
    {
        private IMongoCollection<Question> questions;

        public QuestionSeeder(MongoDbContext context)
        {
            questions = context.Questions;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var existingCount = await questions.CountDocumentsAsync(FilterDefinition<Question>.Empty, cancellationToken: cancellationToken);
            if (existingCount > 0) return;

            List<Question> fakeData = new List<Question>
            {
                new Question(
                    Guid.NewGuid(),
                    new UserInfo(Guid.NewGuid(), "Charlie", "Brown", "https://randomuser.me/api/portraits/men/10.jpg"),
                    new TextValue("What sizes are available?", "QuestionText")
                ),
                new Question(
                    Guid.NewGuid(),
                    new UserInfo(Guid.NewGuid(), "Dana", "White", "https://randomuser.me/api/portraits/women/11.jpg"),
                    new TextValue("Is this item washable?", "QuestionText")
                ),
                new Question(
                    Guid.NewGuid(),
                    new UserInfo(Guid.NewGuid(), "Ella", "Green", "https://randomuser.me/api/portraits/women/12.jpg"),
                    new TextValue("Can I return it if it doesn't fit?", "QuestionText")
                ),
                new Question(
                    Guid.NewGuid(),
                    new UserInfo(Guid.NewGuid(), "Frank", "Black", "https://randomuser.me/api/portraits/men/13.jpg"),
                    new TextValue("Does it come in different colors?", "QuestionText")
                )
            };

            fakeData[0].AddAnswer(new Answer(
                new UserInfo(Guid.NewGuid(), "Alice", "Smith", "https://randomuser.me/api/portraits/women/1.jpg"),
                new TextValue("Available in S, M, L, XL.", "AnswerText")
            ));
            fakeData[0].AddAnswer(new Answer(
                new UserInfo(Guid.NewGuid(), "Bob", "Johnson", "https://randomuser.me/api/portraits/men/2.jpg"),
                new TextValue("Check the product page for exact measurements.", "AnswerText")
            ));

            fakeData[3].AddAnswer(new Answer(
                new UserInfo(Guid.NewGuid(), "Clara", "Davis", "https://randomuser.me/api/portraits/women/3.jpg"),
                new TextValue("Yes, we have red, blue and black options.", "AnswerText")
            ));

            await questions.InsertManyAsync(fakeData, cancellationToken: cancellationToken);
        }
    }
}