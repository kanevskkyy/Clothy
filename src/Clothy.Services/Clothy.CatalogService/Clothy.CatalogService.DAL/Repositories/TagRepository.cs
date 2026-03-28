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
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<Dictionary<Tag, int>> GetTagsWithStockCountAsync(CancellationToken cancellationToken = default)
        {
            List<Tag> tags = await dbSet
                .AsNoTracking()
                .Include(t => t.ClotheTags)
                .ToListAsync(cancellationToken);

            return tags.ToDictionary(
                tag => tag,
                tag => tag.ClotheTags.DistinctBy(ct => ct.ClotheId).Count()
            );
        }

        public async Task<bool> AreAllExistAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
        {
            if (tagIds == null || !tagIds.Any()) return true;

            List<Guid> existingIds = await dbSet
                .Where(tag => tagIds.Contains(tag.Id))
                .Select(tag => tag.Id)
                .ToListAsync(cancellationToken);

            return existingIds.Count == tagIds.Count();
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

        public async  Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default)
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
