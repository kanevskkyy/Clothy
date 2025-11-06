using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.gRPC.Client.Services.Interfaces
{
    public interface IOrderItemValidatorGrpcClient
    {
        Task<ValidateOrderItemsResponse> ValidateOrderItemsAsync(List<OrderItemToValidate> items);
    }
}
