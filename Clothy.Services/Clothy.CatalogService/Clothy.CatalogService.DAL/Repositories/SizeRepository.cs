using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class SizeRepository : GenericRepository<Size>, ISizeRepository
    {
        public SizeRepository(ClothyCatalogDbContext context) : base(context) 
        {

        }

        public async Task<Dictionary<Size, int>> GetSizesCountWithStockAsync(CancellationToken cancellationToken = default)
        {
            List<Size> sizes = await dbSet
                .AsNoTracking()
                .Include(s => s.ClothesStocks)
                .ToListAsync(cancellationToken);

            Dictionary<Size, int> result = new Dictionary<Size, int>();
            foreach (Size size in sizes)
            {
                int clotheItemCount = size.ClothesStocks
                    .Select(stock => stock.ClotheId)
                    .Distinct()
                    .Count();

                result.Add(size, clotheItemCount);
            }

            return result;
        }
        public async Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return await dbSet.AnyAsync(property => property.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            else
            {
                return await dbSet.AnyAsync(property => property.Name.ToLower() == name.ToLower() && property.Id != id, cancellationToken);
            }
        }

        public async Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return await dbSet.AnyAsync(property => property.Slug.ToLower() == slug.ToLower(), cancellationToken);
            }
            else
            {
                return await dbSet.AnyAsync(property => property.Slug.ToLower() == slug.ToLower() && property.Id != id, cancellationToken);
            }
        }
    }
}
