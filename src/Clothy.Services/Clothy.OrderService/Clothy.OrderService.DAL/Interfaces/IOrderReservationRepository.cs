using Clothy.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IOrderReservationRepository : IGenericRepository<OrderReservation>
    {
        Task<int> GetReservedQuantityAsync(Guid clotheId, Guid sizeId, Guid colorId, CancellationToken cancellationToken = default);
        Task<List<OrderReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
        Task<List<OrderReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
