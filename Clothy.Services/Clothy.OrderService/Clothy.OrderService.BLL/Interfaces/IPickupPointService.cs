using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IPickupPointService
    {
        Task<PagedList<PickupPointReadDTO>> GetPagedAsync(PickupPointFilterDTO pickupPointFilterDTO, CancellationToken cancellationToken = default);
        Task<PickupPointReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PickupPointReadDTO> CreateAsync(PickupPointCreateDTO pickupPointCreateDTO, CancellationToken cancellationToken = default);
        Task<PickupPointReadDTO> UpdateAsync(Guid id, PickupPointUpdateDTO pickupPointUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
