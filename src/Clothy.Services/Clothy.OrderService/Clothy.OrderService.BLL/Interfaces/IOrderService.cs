using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Events.UserEvents;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IOrderService
    {
        Task HandleOrderPaidEventAsync(OrderPaidEvent orderPaidEvent, CancellationToken cancellationToken = default);
        Task<PagedList<OrderReadDTO>> GetPagedAsync(OrderFilterDTO filter, ClaimsPrincipal? user = null, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO?> GetByIdAsync(Guid id, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO> CreateAsync(OrderCreateDTO orderCreateDTO, ClaimsPrincipal claimsPrincipal = null, CancellationToken cancellationToken = default);
        Task<OrderDetailDTO?> UpdateStatusAsync(Guid id, OrderUpdateStatusDTO orderUpdateStatusDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
