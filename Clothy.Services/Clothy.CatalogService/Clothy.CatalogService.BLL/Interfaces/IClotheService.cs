using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IClotheService
    {
        Task<List<ClotheSummaryDTO>?> GetTop8MostPopularAsync(CancellationToken cancellationToken = default);
        Task<PriceRangeDTO> GetMinAndMaxPriceAsync(CancellationToken cancellationToken = default);
        Task<PagedList<ClotheSummaryDTO>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> GetDetailBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> CreateAsync(ClotheCreateDTO clotheCreateDTO, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> UpdateAsync(Guid id, ClotheUpdateDTO clotheUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
