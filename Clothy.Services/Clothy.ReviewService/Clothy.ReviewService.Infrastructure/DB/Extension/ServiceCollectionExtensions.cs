using Clothy.ReviewService.Infrastructure.DB.Indexes;
using Clothy.ReviewService.Infrastructure.DB.Seeding;
using Clothy.ReviewService.Infrastructure.DB.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Clothy.ReviewService.Infrastructure.DB.UOW;

namespace Clothy.ReviewService.Infrastructure.DB.Extension
{
    public static class ServiceCollectionExtensions
    {
        private static bool isGuidSerializerRegistered = false;
        private static readonly object _lock = new object();

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigureMongoDbSerialization();

            var connectionString = configuration.GetConnectionString("ClothyReviewsDb");

            MongoDbSettings mongoSettings = new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = "ClothyReviewsDb" 
            };

            services.AddSingleton(mongoSettings);

            MongoDbContext context = new MongoDbContext(mongoSettings);
            services.AddSingleton(context);

            services.AddScoped<IUnitOfWork>(sp =>
            {
                var mongoDb = sp.GetRequiredService<MongoDbContext>().Database;
                return new UnitOfWork(mongoDb);
            });

            services.AddSingleton<IIndexCreationService, IndexCreationService>();
            services.AddSingleton<ReviewSeeder>();
            services.AddSingleton<QuestionSeeder>();

            return services;
        }

        private static void ConfigureMongoDbSerialization()
        {
            lock (_lock)
            {
                if (!isGuidSerializerRegistered)
                {
                    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    isGuidSerializerRegistered = true;
                }
            }
        }

        public static void EnsureIndexes(this IServiceProvider serviceProvider)
        {
            var indexService = serviceProvider.GetRequiredService<IIndexCreationService>();
            indexService.CreateIndexes();
        }
    }
}