using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Helpers;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Specification;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class ClotheItemRepository : GenericRepository<ClotheItem>, IClotheItemRepository
    {
        public ClotheItemRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<ClotheItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbSet
                .Include(property => property.Brand)
                .Include(property => property.ClothyType)
                .Include(property => property.Photos)
                .Include(property => property.Stocks)
                    .ThenInclude(property => property.Color)
                 .Include(property => property.Stocks)
                    .ThenInclude(property => property.Size)
                .Include(property => property.ClotheTags)
                    .ThenInclude(property => property.Tag)
                .Include(property => property.ClotheMaterials)
                    .ThenInclude(property => property.Material)
                 .FirstOrDefaultAsync(property => property.Id == id, cancellationToken); 
        }

        public async Task<PagedList<ClotheItem>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            ClotheItemSpecification specification = new ClotheItemSpecification(parameters);
            IQueryable<ClotheItem> queryable = ApplySpecification(specification);

            int count = await queryable.CountAsync(cancellationToken);
            List<ClotheItem> clotheItems = await queryable
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<ClotheItem>(clotheItems, count, parameters.PageNumber, parameters.PageSize);
        }
    }
}
