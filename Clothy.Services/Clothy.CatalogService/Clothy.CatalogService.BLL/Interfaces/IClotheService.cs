using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IClotheService
    {
        Task<PriceRangeDTO> GetMinAndMaxPriceAsync(CancellationToken cancellationToken = default);
        Task<PagedList<ClotheSummaryDTO>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> CreateAsync(ClotheCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ClotheDetailDTO> UpdateAsync(Guid id, ClotheUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
