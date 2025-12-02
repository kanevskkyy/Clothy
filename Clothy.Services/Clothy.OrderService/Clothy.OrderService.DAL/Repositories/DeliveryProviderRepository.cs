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

            string sql = @" 
                SELECT COUNT(1) 
                FROM delivery_provider 
                WHERE LOWER(name) = LOWER(@Name) 
                  AND (@ExcludeId IS NULL OR id <> @ExcludeId); 
            ";

            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;

            IDbDataParameter nameParam = command.CreateParameter();
            nameParam.ParameterName = "@Name";
            nameParam.Value = name;
            command.Parameters.Add(nameParam);

            IDbDataParameter excludeIdParam = command.CreateParameter();
            excludeIdParam.ParameterName = "@ExcludeId";
            excludeIdParam.Value = excludeId ?? (object)DBNull.Value;
            excludeIdParam.DbType = DbType.Guid; 
            command.Parameters.Add(excludeIdParam);

            DbCommand dbCommand = (DbCommand)command;
            object? result = await dbCommand.ExecuteScalarAsync(cancellationToken);

            int count = Convert.ToInt32(result);
            return count > 0;
        }
    }
}
