using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.OrderService.Domain.Entities;
using Dapper;

namespace Clothy.OrderService.DAL.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "orders")
        {

        }

        public async Task<IEnumerable<OrderSummaryData>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT o.id, o.userfirstname, o.userlastname, o.createdat, o.updatedat,
                       s.id AS StatusId, s.name AS StatusName, s.iconurl AS StatusIconUrl,
                       COALESCE(SUM(oi.price * oi.quantity), 0) AS TotalAmount
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                LEFT JOIN order_item oi ON oi.orderid = o.id
                GROUP BY o.id, s.id, s.name, s.iconurl, o.userfirstname, o.userlastname, o.createdat, o.updatedat
                ORDER BY o.createdat DESC;
            ";

            IEnumerable<OrderSummaryData> result = await connection.QueryAsync<OrderSummaryData, OrderStatus, double, OrderSummaryData>(
                sql,
                (OrderSummaryData order, OrderStatus status, double totalAmount) =>
                {
                    order.Status = status;
                    order.TotalAmount = totalAmount;
                    return order;
                },
                splitOn: "StatusId",
                commandTimeout: 60
            );

            return result;
        }

        public async Task<OrderWithDetailsData?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT o.id, o.userfirstname, o.userlastname, o.createdat, o.updatedat,
                       s.id AS StatusId, s.name AS StatusName, s.iconurl AS StatusIconUrl,
                       COALESCE(SUM(oi.price * oi.quantity), 0) AS TotalAmount
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                LEFT JOIN order_item oi ON oi.orderid = o.id
                WHERE o.id = @Id
                GROUP BY o.id, s.id, s.name, s.iconurl, o.userfirstname, o.userlastname, o.createdat, o.updatedat;
            ";

            OrderWithDetailsData? order = await connection.QuerySingleOrDefaultAsync<OrderWithDetailsData>(
                sql, new 
                { 
                    Id = id 
                });

            if (order == null) return null;
            
            string itemsSql = @"
                SELECT *
                FROM order_item
                WHERE orderid = @Id;
            ";

            IEnumerable<OrderItemData> items = await connection.QueryAsync<OrderItemData>(itemsSql, new 
            { 
                Id = id 
            });
            order.Items = new List<OrderItemData>(items);

            string deliverySql = @"
                SELECT dd.*, dp.id AS DeliveryProviderId, dp.name AS DeliveryProviderName, dp.iconurl AS DeliveryProviderIconUrl,
                       c.id AS CityId, c.name AS CityName
                FROM delivery_detail dd
                JOIN delivery_provider dp ON dd.providerid = dp.id
                JOIN city c ON dd.cityid = c.id
                WHERE dd.orderid = @Id;
            ";

            IEnumerable<DeliveryDetailData> deliveryResult = await connection.QueryAsync<DeliveryDetailData, DeliveryProvider, City, DeliveryDetailData>(
                deliverySql,
                (DeliveryDetailData dd, DeliveryProvider dp, City city) =>
                {
                    dd.DeliveryProvider = dp;
                    dd.City = city;
                    return dd;
                },
                new 
                { 
                    Id = id 
                },
                splitOn: "DeliveryProviderId,CityId"
            );

            List<DeliveryDetailData> deliveryList = new List<DeliveryDetailData>(deliveryResult);
            order.DeliveryDetail = deliveryList.Count > 0 ? deliveryList[0] : null;

            return order;
        }
    }
}