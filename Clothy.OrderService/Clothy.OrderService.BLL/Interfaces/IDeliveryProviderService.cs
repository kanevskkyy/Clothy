using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IDeliveryProviderService
    {
        Task<List<DeliveryProviderReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DeliveryProviderReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DeliveryProviderReadDTO> CreateAsync(DeliveryProviderCreateDTO dto, CancellationToken cancellationToken = default);
        Task<DeliveryProviderReadDTO> UpdateAsync(Guid id, DeliveryProviderUpdateDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
