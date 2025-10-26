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

            var rawResult = await connection.QueryAsync(sql);

            var result = rawResult.Select(r => new OrderSummaryData
            {
                Id = r.id,
                UserFirstName = r.userfirstname,
                UserLastName = r.userlastname,
                CreatedAt = r.createdat,
                UpdatedAt = r.updatedat,
                TotalAmount = (double)r.totalamount,
                Status = new OrderStatus
                {
                    Id = r.statusid,
                    Name = r.statusname,
                    IconUrl = r.statusiconurl,
                    CreatedAt = null,
                    UpdatedAt = null
                }
            });

            return result;
        }


        public async Task<OrderWithDetailsData?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            IDbConnection connection = await GetOpenConnectionAsync();

            string orderSql = @"
                SELECT o.id, o.userfirstname, o.userlastname, o.createdat, o.updatedat,
                       s.id AS Id, s.name AS Name, s.iconurl AS IconUrl,
                       COALESCE(SUM(oi.price * oi.quantity), 0) AS TotalAmount
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                LEFT JOIN order_item oi ON oi.orderid = o.id
                WHERE o.id = @Id
                GROUP BY o.id, s.id, s.name, s.iconurl, o.userfirstname, o.userlastname, o.createdat, o.updatedat;
                ";

            OrderWithDetailsData? order = await connection.QueryAsync<OrderWithDetailsData, OrderStatus, OrderWithDetailsData>(
                orderSql,
                (o, s) =>
                {
                    o.Status = s;
                    return o;
                },
                new { 
                    Id = id 
                },
                splitOn: "Id" 
            ).ContinueWith(t => t.Result.FirstOrDefault());


            if (order == null) return null;

            string itemsSql = @"SELECT * FROM order_item WHERE orderid = @Id;";
            var items = await connection.QueryAsync<OrderItemData>(itemsSql, new 
            { 
                Id = id 
            });
            order.Items = items.ToList();

            string deliverySql = @"
                SELECT dd.id AS DeliveryDetailId, dd.postalindex, dd.phonenumber, 
                       dd.firstname, dd.lastname, dd.middlename, dd.detailsdescription,
                       dd.createdat AS DeliveryCreatedAt, dd.updatedat AS DeliveryUpdatedAt,
                       dp.id AS DeliveryProviderId, dp.name AS DeliveryProviderName, 
                       dp.iconurl AS DeliveryProviderIconUrl,
                       c.id AS CityId, c.name AS CityName
                FROM delivery_detail dd
                JOIN delivery_provider dp ON dd.providerid = dp.id
                JOIN city c ON dd.cityid = c.id
                WHERE dd.orderid = @Id;
            ";

            dynamic? delivery = await connection.QuerySingleOrDefaultAsync<dynamic>(deliverySql, new 
            { 
                Id = id 
            });

            if (delivery != null)
            {
                order.DeliveryDetail = new DeliveryDetailData
                {
                    Id = (Guid)delivery.deliverydetailid,
                    PostalIndex = delivery.postalindex,
                    PhoneNumber = delivery.phonenumber,
                    FirstName = delivery.firstname,
                    LastName = delivery.lastname,
                    MiddleName = delivery.middlename,
                    DetailsDescription = delivery.detailsdescription,
                    CreatedAt = delivery.deliverycreatedat,
                    UpdatedAt = delivery.deliveryupdatedat,
                    DeliveryProvider = new DeliveryProvider
                    {
                        Id = (Guid)delivery.deliveryproviderid,
                        Name = delivery.deliveryprovidername,
                        IconUrl = delivery.deliveryprovidericonurl,  
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = null
                    },
                    City = new City
                    {
                        Id = (Guid)delivery.cityid,
                        Name = delivery.cityname,
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = null
                    }
                };
            }

            return order;
        }
    }
}