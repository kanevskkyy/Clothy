using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IRegionService
    {
        Task<PagedList<RegionReadDTO>> GetPagedAsync(RegionFilterDTO filter, CancellationToken cancellationToken = default);
        Task<RegionReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RegionReadDTO> CreateAsync(RegionCreateDTO regionCreateDTO, CancellationToken cancellationToken = default);
        Task<RegionReadDTO> UpdateAsync(Guid id, RegionUpdateDTO regionUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
