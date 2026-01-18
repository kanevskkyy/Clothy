using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities.Clothe;
using CloudinaryDotNet.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class ClothePopularityRepository : GenericRepository<ClothePopularity>, IClothePopularityRepository
    {
        public ClothePopularityRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<ClothePopularity?> GetClothePopularityByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            return await dbSet.FirstOrDefaultAsync(property => property.ClotheId == clotheId, cancellationToken);
        }

        public async Task<List<ClotheItem>> GetTop8MostPopularAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .Include(property => property.ClotheItem) 
                    .ThenInclude(property => property.Stocks)
                .Include(property => property.ClotheItem)
                    .ThenInclude(property => property.ClotheTags)
                .Include(property => property.ClotheItem)
                    .ThenInclude(property => property.Collection)
                .Include(property => property.ClotheItem)
                    .ThenInclude(property => property.ClothyType)
                .Include(property => property.ClotheItem)
                    .ThenInclude(property => property.Brand)
                .Include(property => property.ClotheItem)
                    .ThenInclude(ci => ci.Photos)
                        .ThenInclude(p => p.Color)
                .OrderByDescending(property => property.SoldCount)
                .Take(8)
                .Select(property => property.ClotheItem!)
                .ToListAsync(cancellationToken);
        }
    }
}
