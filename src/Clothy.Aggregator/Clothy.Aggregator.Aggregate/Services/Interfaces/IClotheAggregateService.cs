using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.DTOs;

namespace Clothy.Aggregator.Aggregate.Services.Interfaces
{
    public interface IClotheAggregateService
    {
        Task<ClotheDetailFullDTO> GetFullClotheDetailAsync(string slug, CancellationToken cancellationToken = default);
    }
}
