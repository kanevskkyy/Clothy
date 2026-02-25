using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.Aggregator.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public class DashboardAggregatorController : ControllerBase
    {
        private IDashboardAggregateService dashboardAggregateService;
        private ILogger<DashboardAggregatorController> logger;

        public DashboardAggregatorController(IDashboardAggregateService dashboardAggregateService, ILogger<DashboardAggregatorController> logger)
        {
            this.dashboardAggregateService = dashboardAggregateService;
            this.logger = logger;
        }

        /// <summary>
        /// Get aggregated dashboard statistics for the store.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Full dashboard statistics including orders, stock, reviews and questions.</returns>
        [HttpGet]
        public async Task<ActionResult<DashboardFullDTO>> GetDashboardStatistics(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting getting aggregated dashboard statistics...");
            DashboardFullDTO result = await dashboardAggregateService.GetDashboardStatisticsAsync(cancellationToken);
           
            return Ok(result);
        }
    }
}
