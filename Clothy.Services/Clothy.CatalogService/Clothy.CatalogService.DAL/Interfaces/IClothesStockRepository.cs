using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IClothesStockRepository : IGenericRepository<ClothesStock>
    {
        Task<ClothesStock?> GetByClotheColorSizeAsync(Guid clotheId, Guid colorId, Guid sizeId, CancellationToken cancellationToken = default);
        Task<ClothesStock?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedList<ClothesStock>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default);
    }
}
