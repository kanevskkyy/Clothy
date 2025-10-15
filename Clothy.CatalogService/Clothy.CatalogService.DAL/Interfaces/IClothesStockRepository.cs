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
    public interface IClothesStockRepository : IGenericRepository<ClothesStock>
    {
        Task<bool> IsSizeAndColorAndClotheIdsExists(Guid sizeId, Guid colorId, Guid clotheId, CancellationToken cancellationToken = default);
        Task<ClothesStock?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedList<ClothesStock>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default);
    }
}
