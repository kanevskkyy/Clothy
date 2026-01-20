using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface ISizeRepository : IGenericRepository<Size>
    {
        Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default);
        Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default);
    }
}
