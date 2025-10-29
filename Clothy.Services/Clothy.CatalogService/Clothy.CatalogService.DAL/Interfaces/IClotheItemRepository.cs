using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.Helpers;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IClotheItemRepository : IGenericRepository<ClotheItem>
    {
        Task<(decimal minPrice, decimal maxPrice)> GetMinAndMaxPriceAsync(CancellationToken cancellationToken = default);
        Task<bool> IsSlugAlreadyExistsAsync(string slug, Guid? id = null, CancellationToken cancellationToken = default);
        Task<PagedList<ClotheItem>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default);
        Task<ClotheItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
