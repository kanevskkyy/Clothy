using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.Indexes
{
    public class IndexCreationService : IIndexCreationService
    {
        private MongoDbContext dbContext;

        public IndexCreationService(MongoDbContext context)
        {
            dbContext = context;
        }

        public void CreateIndexes()
        {
            var reviewCollection = dbContext.Reviews;
            var reviewKeys = Builders<Review>.IndexKeys;

            reviewCollection.Indexes.CreateOne(new CreateIndexModel<Review>(reviewKeys.Ascending(r => r.User.UserId)));
            reviewCollection.Indexes.CreateOne(new CreateIndexModel<Review>(reviewKeys.Ascending(r => r.ClotheItemId)));
            reviewCollection.Indexes.CreateOne(new CreateIndexModel<Review>(reviewKeys.Ascending(r => r.Rating)));

            var questionCollection = dbContext.Questions;
            var questionKeys = Builders<Question>.IndexKeys;

            questionCollection.Indexes.CreateOne(new CreateIndexModel<Question>(questionKeys.Ascending(q => q.User.UserId)));
            questionCollection.Indexes.CreateOne(new CreateIndexModel<Question>(questionKeys.Ascending(q => q.ClotheItemId)));
        }
    }
}
