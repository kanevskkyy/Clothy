using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IColorRepository : IGenericRepository<Color>
    {
        Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default);
        Task<bool> IsHexAlreadyExistsAsync(string hex, Guid? id = null, CancellationToken cancellationToken = default);
        Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default);
        Task<Dictionary<Color, int>> GetColorsCountWithStockAsync(CancellationToken cancellationToken = default);
    }
}
