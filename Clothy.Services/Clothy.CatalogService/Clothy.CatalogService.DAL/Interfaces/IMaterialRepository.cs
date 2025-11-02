using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IMaterialRepository : IGenericRepository<Material>
    {
        Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default);
        Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default);
        Task<Dictionary<Material, int>> GetMaterialsWithStockAsync(CancellationToken cancellationToken = default);
        Task<bool> AreAllExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    }
}
