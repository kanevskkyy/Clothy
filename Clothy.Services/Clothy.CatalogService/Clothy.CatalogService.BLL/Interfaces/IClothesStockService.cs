using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IClothesStockService
    {
        Task<PagedList<ClothesStockReadDTO>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default);
        Task<ClothesStockReadDTO> GetByIdWithDetailsAsync(Guid id,CancellationToken cancellationToken = default);
        Task<ClothesStockReadDTO> CreateAsync(ClothesStockCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ClothesStockReadDTO> UpdateAsync(Guid id, ClothesStockUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
