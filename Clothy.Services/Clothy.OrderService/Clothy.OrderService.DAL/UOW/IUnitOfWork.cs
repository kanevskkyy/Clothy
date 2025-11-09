using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.Interfaces;

namespace Clothy.OrderService.DAL.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderStatusRepository OrderStatuses { get; }
        IDeliveryProviderRepository DeliveryProviders { get; }
        ICityRepository Cities { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IDeliveryDetailRepository DeliveryDetails { get; }
        IRegionRepository Region { get; }
        ISettlementRepository Settlement { get; }

        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
