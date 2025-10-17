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
    public class OrderStatusRepository : GenericRepository<OrderStatus>, IOrderStatusRepository
    {
        public OrderStatusRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "order_status") 
        {

        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COUNT(1)
                FROM order_status
                WHERE LOWER(name) = LOWER(@Name)
                AND (@ExcludeId IS NULL OR id <> @ExcludeId);
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new 
                { 
                    Name = name, ExcludeId = excludeId 
                }, 
                cancellationToken: cancellationToken)
            );

            return count > 0;
        }
    }
}
