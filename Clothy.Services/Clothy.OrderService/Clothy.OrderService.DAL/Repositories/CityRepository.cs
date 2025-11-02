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
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        public CityRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "city")
        {

        }

        public async Task<(IEnumerable<City> Items, int TotalCount)> GetPagedAsync(CityFilterDTO filter, CancellationToken cancellationToken = default)
        {
            IDbConnection connection = await GetOpenConnectionAsync();
            StringBuilder sql = new StringBuilder("SELECT * FROM city WHERE 1=1 ");
            StringBuilder countSql = new StringBuilder("SELECT COUNT(*) FROM city WHERE 1=1 ");
            DynamicParameters parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                sql.Append(" AND LOWER(name) LIKE LOWER(@Name) ");
                countSql.Append(" AND LOWER(name) LIKE LOWER(@Name) ");
                parameters.Add("Name", $"%{filter.Name}%");
            }

            string sortBy = filter.SortBy?.ToLower() ?? "name";
            string direction = filter.SortDescending ? "DESC" : "ASC";
            sql.Append($" ORDER BY {sortBy} {direction} ");

            int skip = (filter.PageNumber - 1) * filter.PageSize;
            sql.Append(" LIMIT @PageSize OFFSET @Skip");
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Skip", skip);

            int totalCount = await connection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
            var items = await connection.QueryAsync<City>(sql.ToString(), parameters);

            return (items, totalCount);
        }



        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancelletionToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT COUNT(1)
                FROM city
                WHERE LOWER(name) = LOWER(@Name)
                AND (@ExcludeId IS NULL OR id <> @ExcludeId);
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new 
                { 
                    Name = name, 
                    ExcludeId = excludeId 
                }, cancellationToken: cancelletionToken)
            );

            return count > 0;
        }
    }
}
