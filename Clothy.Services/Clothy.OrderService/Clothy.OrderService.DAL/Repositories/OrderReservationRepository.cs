using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.Repositories
{
    public class OrderReservationRepository : GenericRepository<OrderReservation>, IOrderReservationRepository
    {
        public OrderReservationRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "orders_reservations")
        {

        }

        public async Task<List<OrderReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT r.* 
                FROM orders_reservations r
                INNER JOIN orders o ON r.orderid = o.id
                INNER JOIN order_status s ON o.statusid = s.id
                WHERE r.expiresat < (NOW() AT TIME ZONE 'utc')
                  AND r.isactive = true
                  AND s.name = 'Awaiting payment'
            ";
            
            IEnumerable<OrderReservation> result = await connection.QueryAsync<OrderReservation>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return result.AsList();
        }

        public async Task<List<OrderReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                            SELECT * 
                            FROM orders_reservations
                            WHERE orderid = @OrderId
            ";

            IEnumerable<OrderReservation> result = await connection.QueryAsync<OrderReservation>(sql, new { OrderId = orderId });
            return result.AsList();
        }

        public async Task<int> GetReservedQuantityAsync(Guid clotheId, Guid sizeId, Guid colorId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COALESCE(SUM(quantity), 0) 
                FROM orders_reservations
                WHERE clotheid = @ClotheId
                  AND sizeid = @SizeId
                  AND colorid = @ColorId
                  AND isactive = true
            ";

            int reservedQuantity = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { ClotheId = clotheId, SizeId = sizeId, ColorId = colorId }, cancellationToken: cancellationToken));
            return reservedQuantity;
        }
    }
}
