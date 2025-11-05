using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Clients.Interfaces
{
    public interface IClotheGrpcClient
    {
        Task<ClotheDetailGrpcResponse> GetClotheByIdAsync(string id);
    }
}
