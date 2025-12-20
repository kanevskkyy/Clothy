using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.Shared.Helpers.Exceptions;
using DnsClient.Internal;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.gRPC.Server.Services
{
    public class GetOrderInfoGrpcServer : OrderServiceGrpc.OrderServiceGrpcBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<GetOrderInfoGrpcServer> logger;

        public GetOrderInfoGrpcServer(IUnitOfWork unitOfWork, ILogger<GetOrderInfoGrpcServer> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<GetOrderInfoResponse> GetOrderInfo(GetOrderInfoRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching data for order with ID: {}", request.OrderId);

            try
            {
                Guid orderId = Guid.Parse(request.OrderId);
                OrderWithDetailsData? order = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId, context.CancellationToken);

                if (order == null) throw new NotFoundException($"Order with ID: {orderId} not found!");

                GetOrderInfoResponse response = new GetOrderInfoResponse()
                {
                    OrderId = orderId.ToString(),
                    Price = order.Items.Sum(orderItem => orderItem.Price).ToString(),
                    UserId = order.UserId.ToString(),
                    Status = order?.Status?.Name
                };

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
