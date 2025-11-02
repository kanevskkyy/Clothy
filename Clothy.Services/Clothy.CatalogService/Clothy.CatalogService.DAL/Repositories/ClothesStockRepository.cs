using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Specification;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class ClothesStockRepository : GenericRepository<ClothesStock>, IClothesStockRepository
    {
        public ClothesStockRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<ClothesStock?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbSet
                .Include(cs => cs.Clothe)
                .Include(cs => cs.Size)
                .Include(cs => cs.Color)
                .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
        }

        public async Task<PagedList<ClothesStock>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            ClothesStockSpecification specification = new ClothesStockSpecification(parameters);
            IQueryable<ClothesStock> queryable = ApplySpecification(specification);

            int count = await queryable.CountAsync(cancellationToken);
            List<ClothesStock> stocks = await queryable
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<ClothesStock>(stocks, count, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<bool> IsSizeAndColorAndClotheIdsExists(Guid sizeId, Guid colorId, Guid clotheId, CancellationToken cancellationToken = default)
        {
            return await dbSet.AnyAsync(property => property.SizeId == sizeId && property.ColorId == colorId && property.ClotheId == clotheId, cancellationToken);
        }
    }
}
