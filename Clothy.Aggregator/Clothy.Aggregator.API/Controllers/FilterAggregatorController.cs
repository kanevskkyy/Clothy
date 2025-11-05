using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.Aggregator.API.Controllers
{
    [ApiController]
    [Route("api/filters")]
    public class FilterAggregatorController : ControllerBase
    {
        private IFilterGrpcClient filterClient;
        private ILogger<FilterAggregatorController> logger;

        public FilterAggregatorController(IFilterGrpcClient filterClient, ILogger<FilterAggregatorController> logger)
        {
            this.filterClient = filterClient;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ClotheFiltersDTO>> GetAllFilters(CancellationToken ct)
        {
            ClotheFiltersDTO? filters = await filterClient.GetAllFiltersAsync(ct);
            if (filters == null)
            {
                logger.LogWarning("Failed to get filters");
                return StatusCode(500, "Could not retrieve filters");
            }

            return Ok(filters);
        }
    }
}
