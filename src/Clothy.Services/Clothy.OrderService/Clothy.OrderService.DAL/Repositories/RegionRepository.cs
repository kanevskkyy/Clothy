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

        public async Task<Region?> GetByRefAsync(string refValue, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT id AS Id, 
                       name AS Name, 
                       ref AS Ref, 
                       createdat AS CreatedAt, 
                       updatedat AS UpdatedAt
                FROM regions
                WHERE ref = @Ref
                LIMIT 1;
            ";

            return await connection.QueryFirstOrDefaultAsync<Region>(
                new CommandDefinition(
                    sql,
                    new { Ref = refValue },
                    cancellationToken: cancellationToken
                )
            );
        }

        public async Task<bool> ExistByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();
            string sql = @"
                SELECT COUNT(1)
                FROM regions
                WHERE LOWER(name) = LOWER(@Name)
                AND (@ExcludeId IS NULL OR id <> @ExcludeId);
            ";

            int count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new
                {
                    Name = name, ExcludeId = excludeId
                },
                cancellationToken: cancellationToken
                )
            );
            return count > 0;
        }
    }
}
