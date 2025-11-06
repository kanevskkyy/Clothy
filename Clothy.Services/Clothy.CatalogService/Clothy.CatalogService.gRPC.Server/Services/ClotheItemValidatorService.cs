using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.UOW;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.gRPC.Server.Services
{
    public class ClotheItemValidatorService : ClotheItemIdValidator.ClotheItemIdValidatorBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<ClotheItemValidatorService> logger;

        public ClotheItemValidatorService(IUnitOfWork unitOfWork, ILogger<ClotheItemValidatorService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<ClotheItemResponse> ValidateClotheItemId(ClotheItemIdToValidate request, ServerCallContext context)
        {
            if (request == null)
            {
                logger.LogWarning("Received null request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(request.ClotheId))
            {
                logger.LogWarning("Received null or empty ClotheItemId");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ClotheItemId cannot be empty"));
            }

            logger.LogInformation("Starting validating ClotheItemId {ClotheItemId}", request.ClotheId);

            try
            {
                ClotheItemResponse clotheItemResponse = new ClotheItemResponse();
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!Guid.TryParse(request.ClotheId, out Guid clotheItemId) || await unitOfWork.ClotheItems.GetByIdAsync(clotheItemId, context.CancellationToken) == null)
                {
                    clotheItemResponse.IsValid = false;
                    clotheItemResponse.ErrorMessage = $"Invalid ClotheItemId: {clotheItemId}. Cannot find in DB or invalid GUID format";
                    logger.LogWarning("Clothe item not found or invalid GUID format: {ClotheId}", clotheItemId);
                }
                else
                {
                    clotheItemResponse.IsValid = true;
                    logger.LogInformation("ClotheItemId {ClotheItemId} is valid", clotheItemId);
                }

                return clotheItemResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while validating ClotheItemId {ClotheId}", request.ClotheId);
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during validation"));
            }
        }
    }
}