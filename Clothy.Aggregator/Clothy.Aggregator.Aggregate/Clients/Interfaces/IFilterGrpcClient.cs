using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.DTOs.Filters;

namespace Clothy.Aggregator.Aggregate.Clients.Interfaces
{
    public interface IFilterGrpcClient
    {
        Task<ClotheFiltersDTO?> GetAllFiltersAsync(CancellationToken ct);
    }
}
