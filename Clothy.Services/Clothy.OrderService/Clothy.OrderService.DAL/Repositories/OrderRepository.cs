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
                       s.id AS Id, s.name AS Name, s.iconurl AS IconUrl
                FROM orders o
                JOIN order_status s ON o.statusid = s.id
                WHERE o.id = @Id;
            ";

            var orderResult = await connection.QueryAsync<OrderWithDetailsData, OrderStatus, OrderWithDetailsData>(
                orderSql,
                (tempOrder, status) =>
                {
                    tempOrder.Status = status;
                    tempOrder.TotalAmount = 0;
                    return tempOrder;
                },
                new { Id = id },
                splitOn: "Id"
            );

            OrderWithDetailsData? order = orderResult.FirstOrDefault();
            if (order == null) return null;

            string itemsSql = @"SELECT * FROM order_item WHERE orderid = @Id;";
            var items = await connection.QueryAsync<OrderItemData>(itemsSql, new { Id = id });
            order.Items = items.ToList();
            order.TotalAmount = order.Items.Sum(item => item.Price * item.Quantity);

            string deliverySql = @"
                SELECT dd.id AS DeliveryDetailId, dd.phonenumber, 
                       dd.firstname, dd.lastname, dd.middlename,
                       dd.createdat AS DeliveryCreatedAt, dd.updatedat AS DeliveryUpdatedAt
                FROM delivery_detail dd
                WHERE dd.orderid = @Id
                ORDER BY dd.createdat DESC
                LIMIT 1;
            ";

            dynamic? delivery = await connection.QueryFirstOrDefaultAsync<dynamic>(deliverySql, new { Id = id });

            if (delivery != null)
            {
                order.DeliveryDetail = new DeliveryDetailData
                {
                    Id = (Guid)delivery.deliverydetailid,
                    PhoneNumber = delivery.phonenumber,
                    FirstName = delivery.firstname,
                    LastName = delivery.lastname,
                    MiddleName = delivery.middlename,
                    CreatedAt = delivery.deliverycreatedat,
                    UpdatedAt = delivery.deliveryupdatedat,
                };
            }

            return order;
        }
    }
}