using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface ICollectionRepository : IGenericRepository<Collection>
    {
        Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default);
        Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default);
        Task<Dictionary<Collection, int>> GetCollectionsCountWithStockAsync(CancellationToken cancellationToken = default);
    }
}
