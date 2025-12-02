using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.gRPC.Client.Services.Interfaces
{
    public interface ICheckUserPurchasedClotheGrpcClient
    {
        Task<CheckUserPurchasedResponse> CheckUserPurchasedAsync(CheckUserPurchasedRequest checkUserPurchasedRequest);
    }
}
