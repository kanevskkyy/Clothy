using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.gRPC.Client.Services
{
    public class CheckUserPurchasedClotheGrpcClient : ICheckUserPurchasedClotheGrpcClient
    {
        private CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient checkUserPurchasedGrpc;
        private ILogger<CheckUserPurchasedClotheGrpcClient> logger;

        public CheckUserPurchasedClotheGrpcClient(CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient checkUserPurchasedGrpc, ILogger<CheckUserPurchasedClotheGrpcClient> logger)
        {
            this.checkUserPurchasedGrpc = checkUserPurchasedGrpc;
            this.logger = logger;
        }

        public async Task<CheckUserPurchasedResponse> CheckUserPurchasedAsync(CheckUserPurchasedRequest checkUserPurchasedRequest)
        {
            if(checkUserPurchasedRequest == null || string.IsNullOrWhiteSpace(checkUserPurchasedRequest.UserId) || string.IsNullOrWhiteSpace(checkUserPurchasedRequest.ClotheId))
            {
                logger.LogWarning("Attempted to validate empty or null clotheItemId or userId");
            }

            try
            {
                logger.LogInformation("Sending CheckUserPurchased request: UserId={UserId}, ClotheId={ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);

                CheckUserPurchasedResponse response = await checkUserPurchasedGrpc.CheckUserPurchasedAsync(checkUserPurchasedRequest);
                
                if (response.Purchased) logger.LogInformation("User {UserId} HAS purchased ClotheItem {ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);
                else logger.LogWarning("User {UserId} has NOT purchased ClotheItem {ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);

                return response;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
            {
                logger.LogWarning(ex, "Invalid argument sent to gRPC service: UserId={UserId}, ClotheId={ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);
                throw;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while checking purchase for UserId={UserId}, ClotheId={ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while checking purchase for UserId={UserId}, ClotheId={ClotheId}", checkUserPurchasedRequest.UserId, checkUserPurchasedRequest.ClotheId);
                throw;
            }
        }
    }
}
