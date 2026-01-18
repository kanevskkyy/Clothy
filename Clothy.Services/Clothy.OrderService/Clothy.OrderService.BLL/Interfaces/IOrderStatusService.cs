using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IOrderStatusService
    {
        Task<List<OrderStatusReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OrderStatusReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrderStatusReadDTO> CreateAsync(OrderStatusCreateDTO orderStatusCreateDTO, CancellationToken cancellationToken = default);
        Task<OrderStatusReadDTO> UpdateAsync(Guid id, OrderStatusUpdateDTO orderStatusUpdateDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
