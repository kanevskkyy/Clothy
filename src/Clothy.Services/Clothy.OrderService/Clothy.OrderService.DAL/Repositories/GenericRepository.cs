using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Dapper;

namespace Clothy.OrderService.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected IConnectionFactory connectionFactory;
        protected string tableName;

        public GenericRepository(IConnectionFactory connectionFactory, string tableName)
        {
            this.connectionFactory = connectionFactory;
            this.tableName = tableName;
        }

        protected virtual async Task<IDbConnection> GetOpenConnectionAsync()
        {
            IDbConnection databaseConnection = connectionFactory.CreateConnection();

            if (databaseConnection.State != ConnectionState.Open) databaseConnection.Open();
            return databaseConnection;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            string sqlCode = $"SELECT * FROM {tableName}";

            return await databaseConnection.QueryAsync<T>(new CommandDefinition(
                sqlCode, 
                transaction: transaction, 
                cancellationToken: cancellationToken));
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            string sqlCode = $"SELECT * FROM {tableName} WHERE id = @Id";

            return await databaseConnection.QuerySingleOrDefaultAsync<T>(
                new CommandDefinition(sqlCode, new 
                { 
                    Id = id 
                }, 
                transaction: transaction, 
                cancellationToken: cancellationToken));
        }

        public virtual async Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            IEnumerable<string> columns = GetColumns(entity);

            string columnsString = string.Join(", ", columns);
            string paramsString = string.Join(", ", columns.Select(c => "@" + c));
            string sqlCode = $"INSERT INTO {tableName} ({columnsString}) VALUES ({paramsString}) RETURNING id";

            Guid insertedId = await databaseConnection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    sqlCode,
                    entity,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            typeof(T).GetProperty("Id")?.SetValue(entity, insertedId);

            return insertedId;
        }

        public virtual async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            IEnumerable<string> columns = GetColumns(entity);
            
            string setString = string.Join(", ", columns.Select(c => $"{c} = @{c}"));
            string sqlCode = $"UPDATE {tableName} SET {setString} WHERE id = @Id RETURNING *";

            return await databaseConnection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
                sqlCode, 
                entity, 
                transaction: transaction, 
                cancellationToken: cancellationToken));
        }

        public virtual async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            string sqlCode = $"DELETE FROM {tableName} WHERE id = @Id";

            return await databaseConnection.ExecuteAsync(new CommandDefinition(
                sqlCode, 
                new {
                    Id = id 
                }, 
                transaction: transaction, 
                cancellationToken: cancellationToken));
        }

        public async Task AddWithoutReturningAsync(T entity, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
        {
            using IDbConnection databaseConnection = await GetOpenConnectionAsync();
            IEnumerable<string> columns = GetColumns(entity);

            string columnsString = string.Join(", ", columns);
            string paramsString = string.Join(", ", columns.Select(c => "@" + c));
            string sqlCode = $"INSERT INTO {tableName} ({columnsString}) VALUES ({paramsString})";

            await databaseConnection.ExecuteAsync(new CommandDefinition(
                sqlCode, 
                entity, 
                transaction: transaction, 
                cancellationToken: cancellationToken));
        }

        private static IEnumerable<string> GetColumns(T entity)
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "Id")
                .Where(p => !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
                            || p.PropertyType == typeof(string))
                .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string))
                .Select(p => p.Name.ToLower());
        }

    }
}
