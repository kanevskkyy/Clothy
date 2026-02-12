using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        Task<(bool hasPurchased, string? clotheName, string? clothePhotoUrl)> HasUserPurchasedClotheAsync(Guid userId, Guid clotheId, CancellationToken cancellationToken = default);
        Task<List<OrderItem>> GetByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default);
    }
}
