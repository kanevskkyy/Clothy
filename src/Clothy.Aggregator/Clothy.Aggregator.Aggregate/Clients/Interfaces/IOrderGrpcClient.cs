using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Clients.Interfaces
{
    public interface IOrderGrpcClient
    {
        Task<OrderStats> GetOrderStatsAsync(CancellationToken cancellationToken = default);
    }
}