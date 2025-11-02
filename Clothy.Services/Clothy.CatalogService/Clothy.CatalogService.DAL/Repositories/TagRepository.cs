using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
                .Include(property => property.ClotheTags)
                .ToListAsync(cancellationToken);

            Dictionary<Tag, int> result = new Dictionary<Tag, int>();

            foreach (Tag tag in tags)
            {
                int quantityCount = tag.ClotheTags.Count;
                result.Add(tag, quantityCount);
            }

            return result;
        }

        public async Task<bool> AreAllExistAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
        {
            if (tagIds == null || !tagIds.Any()) return true;

            var existingIds = await dbSet
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
    }
}
