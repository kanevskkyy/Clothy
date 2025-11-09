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
    public class SettlementRepository : GenericRepository<Settlement>, ISettlementRepository
    {
        public SettlementRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "settlements")
        {

        }

        public async Task<bool> ExistsByNameAndRegionIdAsync(string name, Guid regionId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COUNT(1)
                FROM settlements
                WHERE LOWER(name) = LOWER(@Name)
                AND (@ExcludeId IS NULL OR id <> @ExcludeId)
                AND regionid = @RegionId;
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new
                {
                    Name = name,
                    ExcludeId = excludeId,
                    RegionId = regionId,
                },
                cancellationToken: cancellationToken
                )
            );
            return count > 0;
        }

        public async Task<(IEnumerable<Settlement>, int totalCount)> GetPagedAsync(SettlementFilterDTO settlementFilterDTO, CancellationToken cancellationToken = default)
        {
            using IDbConnection dbConnection = await GetOpenConnectionAsync();
            
            StringBuilder sql = new StringBuilder("SELECT * FROM settlements WHERE 1=1 ");
            StringBuilder countSql = new StringBuilder("SELECT COUNT(*) FROM settlements WHERE 1=1 ");
            DynamicParameters parameters = new DynamicParameters();

            if (settlementFilterDTO.RegionId.HasValue)
            {
                sql.Append(" AND regionid = @RegionId");
                countSql.Append(" AND regionid = @RegionId");
                parameters.Add("RegionId", settlementFilterDTO.RegionId.Value);
            }

            string sortBy = settlementFilterDTO.SortBy?.ToLower() ?? "name";
            string direction = settlementFilterDTO.SortDescending ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortBy} {direction} ");

            int skip = (settlementFilterDTO.PageNumber - 1) * settlementFilterDTO.PageSize;
            sql.Append(" LIMIT @PageSize OFFSET @Skip");
            parameters.Add("PageSize", settlementFilterDTO.PageSize);
            parameters.Add("Skip", skip);


            int totalCount = await dbConnection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
            var items = await dbConnection.QueryAsync<Settlement>(sql.ToString(), parameters);

            return (items, totalCount);
        }
    }
}
