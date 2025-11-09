using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Dapper;

namespace Clothy.OrderService.DAL.Repositories
{
    public class PickupPointRepository : GenericRepository<PickupPoints>, IPickupPointRepository
    {
        public PickupPointRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "pickup_points")
        {

        }

        public async Task<bool> ExistsByAddressAndProviderIdAsync(string address, Guid deliveryProviderId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COUNT(1)
                FROM pickup_points
                WHERE LOWER(address) = LOWER(@Address)
                AND deliveryproviderid = @DeliveryProviderId
                AND (@ExcludeId IS NULL OR id <> @ExcludeId);
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new
                {
                    Address = address,
                    DeliveryProviderId = deliveryProviderId,
                    ExcludeId = excludeId
                },
                cancellationToken: cancellationToken)
            );

            return count > 0;
        }

        public async Task<(IEnumerable<PickupPoints>, int totalCount)> GetPagedAsync(PickupPointFilterDTO filterDTO, CancellationToken cancellationToken = default)
        {
            using IDbConnection dbConnection = await GetOpenConnectionAsync();

            StringBuilder sql = new StringBuilder("SELECT * FROM pickup_points WHERE 1=1 ");
            StringBuilder countSql = new StringBuilder("SELECT COUNT(*) FROM pickup_points WHERE 1=1 ");
            DynamicParameters parameters = new DynamicParameters();

            if (filterDTO.DeliveryProviderId.HasValue)
            {
                sql.Append(" AND deliveryproviderid = @DeliveryProviderId");
                countSql.Append(" AND deliveryproviderid = @DeliveryProviderId");
                parameters.Add("DeliveryProviderId", filterDTO.DeliveryProviderId.Value);
            }

            string sortBy = filterDTO.SortBy?.ToLower() ?? "address";
            string direction = filterDTO.SortDescending ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortBy} {direction} ");

            int skip = (filterDTO.PageNumber - 1) * filterDTO.PageSize;
            sql.Append(" LIMIT @PageSize OFFSET @Skip");
            parameters.Add("PageSize", filterDTO.PageSize);
            parameters.Add("Skip", skip);

            int totalCount = await dbConnection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
            var items = await dbConnection.QueryAsync<PickupPoints>(sql.ToString(), parameters);

            return (items, totalCount);
        }
    }
}
