using Clothy.Aggregator.Aggregate.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Services.Interfaces
{
    public interface IDashboardAggregateService
    {
        Task<DashboardFullDTO> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default);
    }
}