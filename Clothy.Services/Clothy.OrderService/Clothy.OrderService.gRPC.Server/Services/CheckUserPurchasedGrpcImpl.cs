using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.UOW;
using DnsClient.Internal;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.gRPC.Server.Services
{
    public class CheckUserPurchasedGrpcImpl : CheckUserPurchasedGrpc.CheckUserPurchasedGrpcBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<CheckUserPurchasedGrpcImpl> logger;

        public CheckUserPurchasedGrpcImpl(IUnitOfWork unitOfWork, ILogger<CheckUserPurchasedGrpcImpl> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<CheckUserPurchasedResponse> CheckUserPurchased(CheckUserPurchasedRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting checking whether user ordered clothe with ID: {ClotheId}...", request.ClotheId);

            try
            {
                (bool hasPurchased, string? clotheName, string? clothePhotoUrl) result = await unitOfWork.OrderItems.HasUserPurchasedClotheAsync(Guid.Parse(request.UserId), Guid.Parse(request.ClotheId), context.CancellationToken);

                logger.LogInformation("Succesfully checked whether user ordered clothe!");

                CheckUserPurchasedResponse response = new CheckUserPurchasedResponse();
                response.Purchased = result.hasPurchased;
                response.ClotheName = response.ClotheName;
                response.ClothePhotoURL = response.ClothePhotoURL;

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while reading...");
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
            }
        }
    }
}
