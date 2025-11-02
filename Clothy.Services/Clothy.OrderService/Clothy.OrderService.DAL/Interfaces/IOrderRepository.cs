using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<(IEnumerable<OrderSummaryData> Items, int TotalCount)> GetPagedAsync(OrderFilterDTO filter, CancellationToken cancellationToken = default);
        Task<OrderWithDetailsData?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
