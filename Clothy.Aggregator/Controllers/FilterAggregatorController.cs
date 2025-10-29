using Clothy.Aggregator.DTOs.Filters;
using Clothy.Aggregator.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.Aggregator.Controllers
{
    [ApiController]
    [Route("api/filters")]
    public class FilterAggregatorController : ControllerBase
    {
        private FilterAggregatorService filterService;
        private ILogger<FilterAggregatorController> logger;

        public FilterAggregatorController(FilterAggregatorService filterService, ILogger<FilterAggregatorController> logger)
        {
            this.filterService = filterService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ClotheFiltersDTO>> GetAllFilters(CancellationToken ct)
        {
            ClotheFiltersDTO? filters = await filterService.GetAllFiltersAsync(ct);
            if (filters == null)
            {
                logger.LogWarning("Failed to get filters");
                return StatusCode(500, "Could not retrieve filters");
            }

            return Ok(filters);
        }
    }
}
