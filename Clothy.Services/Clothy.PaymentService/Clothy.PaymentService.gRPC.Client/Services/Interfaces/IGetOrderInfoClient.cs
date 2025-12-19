using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.gRPC.Client.Services.Interfaces
{
    public interface IGetOrderInfoClient
    {
        Task<GetOrderInfoResponse> GetOrderInfoAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
