using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;

namespace Clothy.OrderService.DAL.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private IConnectionFactory connectionFactory;
        private IDbConnection? connection;
        private IDbTransaction? transaction;

        public IDeliveryProviderRepository DeliveryProviders { get; }
        public IOrderRepository Orders { get; }
        public IOrderItemRepository OrderItems { get; }
        public IDeliveryDetailRepository DeliveryDetails { get; }
        public IRegionRepository Region { get; }
        public ISettlementRepository Settlement { get; }
        public IPickupPointRepository PickupPoint { get; }
        public IOrderReservationRepository OrderReservation { get; }

        public IDbConnection Connection => connection ??= connectionFactory.CreateConnection();
        public IDbTransaction? Transaction => transaction;


        public UnitOfWork(
            IConnectionFactory connectionFactory,
            IDeliveryProviderRepository deliveryProviders,
            IOrderRepository orders,
            IOrderItemRepository orderItems,
            IDeliveryDetailRepository deliveryDetails,
            IRegionRepository region,
            ISettlementRepository settlement,
            IPickupPointRepository pickupPoint,
            IOrderReservationRepository orderReservation)
        {
            this.connectionFactory = connectionFactory;
            DeliveryProviders = deliveryProviders;
            Orders = orders;
            OrderItems = orderItems;
            DeliveryDetails = deliveryDetails;
            Region = region;
            Settlement = settlement;
            PickupPoint = pickupPoint;
            OrderReservation = orderReservation;
        }

        public async Task BeginTransactionAsync()
        {
            if (connection == null) connection = connectionFactory.CreateConnection();
            if (connection.State != ConnectionState.Open) connection.Open();

            transaction = connection.BeginTransaction();
            await Task.CompletedTask;
        }

        public async Task CommitAsync()
        {
            try
            {
                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
                transaction = null;
            }

            await Task.CompletedTask;
        }

        public async Task RollbackAsync()
        {
            transaction?.Rollback();
            transaction?.Dispose();
            transaction = null;

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            transaction?.Dispose();
            connection?.Dispose();
        }
    }
}
