using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.gRPC.Client.Services.Interfaces
{
    public interface IBasketGrpcClient
    {
        Task<GetUserBasketResponse> GetUserBasketAsync(Guid userId);
        Task<ClearUserBasketResponse> ClearUserBasketAsync(Guid userId);
    }
}
