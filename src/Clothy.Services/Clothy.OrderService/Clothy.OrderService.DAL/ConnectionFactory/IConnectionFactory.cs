using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.ConnectionFactory
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
