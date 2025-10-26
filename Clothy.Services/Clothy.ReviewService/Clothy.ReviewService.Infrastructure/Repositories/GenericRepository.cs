using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Exceptions;
using Clothy.ReviewService.Domain.Interfaces.Repositories;
using Clothy.ReviewService.Infrastructure.DB;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected IMongoCollection<T> collection;
        protected IClientSessionHandle? session;

        public GenericRepository(MongoDbContext context, IClientSessionHandle? session = null)
        {
            collection = typeof(T).Name switch
            {
                "Review" => (IMongoCollection<T>)context.Reviews,
                "Question" => (IMongoCollection<T>)context.Questions,
                _ => throw new DomainException($"Collection for type {typeof(T).Name} is not defined.")
            };
            this.session = session;
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (session != null) await collection.InsertOneAsync(session, entity, cancellationToken: cancellationToken);
            else await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

            return entity;
        }

        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await collection.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await collection.ReplaceOneAsync(e => e.Id == entity.Id, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await collection.DeleteOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await collection.Find(_ => true).ToListAsync(cancellationToken);
        }
    }
}
