using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Dapper;

namespace Clothy.OrderService.DAL.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "order_item")
        {

        }

        public async Task<List<OrderItem>> GetByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT *
                FROM order_item
                WHERE clotheid = @ClotheId";

            IEnumerable<OrderItem> orderItems = await connection.QueryAsync<OrderItem>(
                new CommandDefinition(
                    sql,
                    new
                    { 
                        ClotheId = clotheId 
                    },
                    cancellationToken: cancellationToken
                )
            );

            return orderItems.ToList();
        }

        public async Task<bool> HasUserPurchasedClotheAsync(Guid userId, Guid clotheId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT 1
                FROM order_item oi
                INNER JOIN orders o ON oi.orderid = o.id
                WHERE o.userid = @UserId
                  AND oi.clotheid = @ClotheId
                LIMIT 1;"; 

            int? result = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    sql,
                    new { UserId = userId, ClotheId = clotheId },
                    cancellationToken: cancellationToken
                )
            );

            return result.HasValue;
        }
    }
}
