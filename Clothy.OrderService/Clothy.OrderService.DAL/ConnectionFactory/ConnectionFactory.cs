using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Clothy.OrderService.DAL.ConnectionFactory
{
    public class ConnectionFactory : IConnectionFactory
    {
        private string connectionString;

        public ConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
