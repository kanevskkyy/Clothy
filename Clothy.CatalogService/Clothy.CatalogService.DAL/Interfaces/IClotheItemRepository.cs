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
        Task<PagedList<ClotheItem>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default);
        Task<ClotheItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
