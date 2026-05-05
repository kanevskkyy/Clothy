using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class ClotheGrpcClient : IClotheGrpcClient
    {
        private ClotheServiceGrpc.ClotheServiceGrpcClient client;
        private ILogger<ClotheGrpcClient> logger;

        public ClotheGrpcClient(ClotheServiceGrpc.ClotheServiceGrpcClient client, ILogger<ClotheGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<ClotheDetailGrpcResponse> GetClotheBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            ClotheGrpcRequest request = new ClotheGrpcRequest { 
                Slug = slug
            };
            ClotheDetailGrpcResponse response = await client.GetClotheBySlugAsync(request, cancellationToken: cancellationToken);
            return response;
        }
    }
}
