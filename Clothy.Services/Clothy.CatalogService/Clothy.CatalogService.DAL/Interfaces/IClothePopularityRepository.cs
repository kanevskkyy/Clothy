using Clothy.CatalogService.Domain.Entities.Clothe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IClothePopularityRepository : IGenericRepository<ClothePopularity>
    {
        Task<ClothePopularity?> GetClothePopularityByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default);
        Task<List<ClotheItem>> GetTop8MostPopularAsync(CancellationToken cancellationToken = default);
    }
}
