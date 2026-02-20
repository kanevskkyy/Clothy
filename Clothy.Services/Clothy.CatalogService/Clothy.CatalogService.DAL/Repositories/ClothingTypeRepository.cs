using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class ClothingTypeRepository : GenericRepository<ClothingType>, IClothingTypeRepository
    {
        public ClothingTypeRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbSet.AnyAsync(property => property.Id == id, cancellationToken);
        }

        public async Task<Dictionary<ClothingType, int>> GetClothingTypeCountWithStockAsync(CancellationToken cancellationToken = default)
        {
            List<ClothingType> clothingTypes = await dbSet
                .AsNoTracking()
                .Include(clothingType => clothingType.Items)
                .ToListAsync(cancellationToken);

            return clothingTypes.ToDictionary(
                clothingType => clothingType,
                clothingType => clothingType.Items.Count
            );
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