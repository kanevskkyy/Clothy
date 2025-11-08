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
    public class RegionRepository : GenericRepository<Region>, IRegionRepository
    {
        public RegionRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "regions")
        {

        }

        public async Task<(IEnumerable<Region> Items, int TotalCount)> GetPagedAsync(RegionFilterDTO filter, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            StringBuilder sql = new StringBuilder("SELECT * FROM regions WHERE 1=1 ");
            StringBuilder countSql = new StringBuilder("SELECT COUNT(*) FROM regions WHERE 1=1 ");
            DynamicParameters parameters = new DynamicParameters();

            if (filter.CityId.HasValue)
            {
                sql.Append(" AND cityid = @CityId ");
                countSql.Append(" AND cityid = @CityId ");
                parameters.Add("CityId", filter.CityId.Value);
            }

            string sortBy = filter.SortBy?.ToLower() ?? "name";
            string direction = filter.SortDescending ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortBy} {direction} ");

            int skip = (filter.PageNumber - 1) * filter.PageSize;
            sql.Append(" LIMIT @PageSize OFFSET @Skip");
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Skip", skip);

            int totalCount = await connection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
            var items = await connection.QueryAsync<Region>(sql.ToString(), parameters);

            return (items, totalCount);
        }

        public async Task<bool> ExistByNameAndCityIdAsync(string name, Guid cityId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COUNT(1)
                FROM regions
                WHERE LOWER(name) = LOWER(@Name)
                AND (@ExcludeId IS NULL OR id <> @ExcludeId)
                AND cityid = @CityId;
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new
                {
                    Name = name, ExcludeId = excludeId, CityId = cityId,
                },
                cancellationToken: cancellationToken
                )
            );
            return count > 0;
        }
    }
}
