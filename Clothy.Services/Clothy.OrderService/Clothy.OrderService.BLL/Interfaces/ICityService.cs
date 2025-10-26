using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.DTOs.FilterDTOs;
using Clothy.OrderService.BLL.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface ICityService
    {
        Task<PagedList<CityReadDTO>> GetPagedAsync(CityFilterDTO filter, CancellationToken cancellationToken = default);
        Task<CityReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CityReadDTO> CreateAsync(CityCreateDTO dto, CancellationToken cancellationToken = default);
        Task<CityReadDTO> UpdateAsync(Guid id, CityUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
