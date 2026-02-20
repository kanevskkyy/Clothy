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
    public class MaterialRepository : GenericRepository<Material>, IMaterialRepository
    {
        public MaterialRepository(ClothyCatalogDbContext context) : base(context) 
        {

        }

        public async Task<Dictionary<Material, int>> GetMaterialsWithStockAsync(CancellationToken cancellationToken = default)
        {
            List<Material> materials = await dbSet
                .AsNoTracking()
                .Include(m => m.ClotheMaterials)
                .ToListAsync(cancellationToken);

            return materials.ToDictionary(
                material => material,
                material => material.ClotheMaterials.DistinctBy(cm => cm.ClotheId).Count()
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

        public async Task<bool> AreAllExistAsync(IEnumerable<Guid> materialIds, CancellationToken cancellationToken = default)
        {
            if (materialIds == null || !materialIds.Any()) return true;

            var existingIds = await dbSet
                .Where(m => materialIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            return existingIds.Count == materialIds.Count();
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
