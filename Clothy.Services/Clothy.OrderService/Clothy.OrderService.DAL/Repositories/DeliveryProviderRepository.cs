using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Dapper;

namespace Clothy.OrderService.DAL.Repositories
{
    public class DeliveryProviderRepository : GenericRepository<DeliveryProvider>, IDeliveryProviderRepository
    {
        public DeliveryProviderRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "delivery_provider")
        {

        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = excludeId.HasValue
                    ? @"SELECT COUNT(1) 
                FROM delivery_provider
                WHERE LOWER(name) = LOWER(@Name)
                  AND id <> @ExcludeId"
                    : @"SELECT COUNT(1) 
                FROM delivery_provider
                WHERE LOWER(name) = LOWER(@Name)";

            using IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;

            IDbDataParameter nameParam = command.CreateParameter();
            nameParam.ParameterName = "@Name";
            nameParam.Value = name;
            command.Parameters.Add(nameParam);

            if (excludeId.HasValue)
            {
                IDbDataParameter excludeParam = command.CreateParameter();
                excludeParam.ParameterName = "@ExcludeId";
                excludeParam.DbType = DbType.Guid;
                excludeParam.Value = excludeId.Value;
                command.Parameters.Add(excludeParam);
            }

            Object? result = await ((DbCommand)command).ExecuteScalarAsync(cancellationToken);
            int count = Convert.ToInt32(result);

            return count > 0;
        }

        public async Task<DeliveryProvider> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string sql = @"
                SELECT *
                FROM delivery_provider
                WHERE LOWER(name) = LOWER(@Name);
            ";
            DeliveryProvider? deliveryProvider = await connection.QueryFirstOrDefaultAsync<DeliveryProvider>(
                new CommandDefinition (
                    sql, 
                    new {Name = name},
                    cancellationToken: cancellationToken
                ));

            return deliveryProvider!;
        }
    }
}
