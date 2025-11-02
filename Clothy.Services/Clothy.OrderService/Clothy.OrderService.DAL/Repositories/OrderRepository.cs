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
using Clothy.OrderService.DAL.FilterDTOs;

namespace Clothy.OrderService.DAL.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "orders")
        {

        }

        public async Task<(IEnumerable<OrderSummaryData> Items, int TotalCount)> GetPagedAsync(OrderFilterDTO filter, CancellationToken cancellationToken = default)
        {
            IDbConnection connection = await GetOpenConnectionAsync();

            StringBuilder sql = new StringBuilder(@"
                SELECT o.id, o.userfirstname, o.userlastname, o.createdat, o.updatedat,
                       s.id AS StatusId, s.name AS StatusName, s.iconurl AS StatusIconUrl,
                       COALESCE(SUM(oi.price * oi.quantity), 0) AS TotalAmount
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                LEFT JOIN order_item oi ON oi.orderid = o.id
            ");

                    StringBuilder countSql = new StringBuilder(@"
                SELECT COUNT(DISTINCT o.id)
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                LEFT JOIN order_item oi ON oi.orderid = o.id
            ");

            List<string> whereClauses = new List<string>();
            DynamicParameters parameters = new DynamicParameters();

            if (filter.StatusId.HasValue)
            {
                whereClauses.Add("o.statusid = @StatusId");
                parameters.Add("@StatusId", filter.StatusId);
            }

            if (filter.UserId.HasValue)
            {
                whereClauses.Add("o.userid = @UserId");
                parameters.Add("@UserId", filter.UserId);
            }

            if (whereClauses.Any())
            {
                string where = " WHERE " + string.Join(" AND ", whereClauses);
                sql.Append(where);
                countSql.Append(where);
            }

            sql.Append(@" GROUP BY o.id, s.id, s.name, s.iconurl, o.userfirstname, o.userlastname, o.createdat, o.updatedat");

            string sortBy = filter.SortBy?.ToLower() ?? "createdat";
            string sortColumn = sortBy switch
            {
                "createdat" => "o.createdat",
                "statusid" => "o.statusid",
                "totalamount" => "TotalAmount",
                _ => "o.createdat"
            };
            string direction = filter.SortDescending ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortColumn} {direction}");

            int skip = (filter.PageNumber - 1) * filter.PageSize;
            sql.Append(" LIMIT @PageSize OFFSET @Skip");
            parameters.Add("@PageSize", filter.PageSize);
            parameters.Add("@Skip", skip);

            int totalCount = await connection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
            var rows = await connection.QueryAsync(sql.ToString(), parameters);

            var items = rows.Select(r => new OrderSummaryData
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
                    IconUrl = r.statusiconurl
                }
            });

            return (items, totalCount);
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