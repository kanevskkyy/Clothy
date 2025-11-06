using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.gRPC.Client.Services
{
    public class ClotheItemIdValidatorGrpcClient : IClotheItemIdValidatorGrpcClient
    {
        private ClotheItemIdValidator.ClotheItemIdValidatorClient client;
        private ILogger<ClotheItemIdValidatorGrpcClient> logger;

        public ClotheItemIdValidatorGrpcClient(ClotheItemIdValidator.ClotheItemIdValidatorClient client, ILogger<ClotheItemIdValidatorGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<ClotheItemResponse> ValidateClotheItemIdAsync(ClotheItemIdToValidate clotheItemIdToValidate)
        {
            if (clotheItemIdToValidate == null || string.IsNullOrWhiteSpace(clotheItemIdToValidate.ClotheId)) logger.LogWarning("Attempted to validate empty or null clotheItemId");

            try
            {
                logger.LogInformation("Sending validation request for ClotheItemId: {ClotheId}", clotheItemIdToValidate.ClotheId);

                ClotheItemResponse response = await client.ValidateClotheItemIdAsync(clotheItemIdToValidate);

                if (response.IsValid) logger.LogInformation("ClotheItemId {ClotheId} is valid", clotheItemIdToValidate.ClotheId);
                else logger.LogWarning("ClotheItemId {ClotheId} is invalid: {Error}", clotheItemIdToValidate.ClotheId, response.ErrorMessage); 
                
                return response;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
            {
                logger.LogWarning(ex, "Invalid argument sent to gRPC service for ClotheItemId {ClotheId}", clotheItemIdToValidate.ClotheId);
                throw;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while validating ClotheItemId {ClotheId}", clotheItemIdToValidate.ClotheId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while validating ClotheItemId {ClotheId}", clotheItemIdToValidate.ClotheId);
                throw;
            }

        }
    }
}
