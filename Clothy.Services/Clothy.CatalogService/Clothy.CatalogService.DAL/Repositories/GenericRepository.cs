using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Ardalis.Specification;
using Clothy.CatalogService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Clothy.CatalogService.DAL.DB;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected ClothyCatalogDbContext context;
        protected DbSet<T> dbSet;

        public GenericRepository(ClothyCatalogDbContext context)
        {
            this.context = context;
            dbSet = this.context.Set<T>();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await dbSet.AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public IQueryable<T> ApplySpecification(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            SpecificationEvaluator evaluator = new SpecificationEvaluator();
            return evaluator
                .GetQuery(dbSet.AsQueryable(), spec)
                .AsSplitQuery();
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }
    }
}
