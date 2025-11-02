using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IOrderService
    {
        Task<PagedList<OrderReadDTO>> GetPagedAsync(OrderFilterDTO filter, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO> CreateAsync(OrderCreateDTO dto, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO?> UpdateStatusAsync(Guid id, OrderUpdateStatusDTO dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
