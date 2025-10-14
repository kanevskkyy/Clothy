using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IColorRepository : IGenericRepository<Color>
    {
        Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default);
        Task<Dictionary<Color, int>> GetColorsCountWithStockAsync(CancellationToken cancellationToken = default);
    }
}
