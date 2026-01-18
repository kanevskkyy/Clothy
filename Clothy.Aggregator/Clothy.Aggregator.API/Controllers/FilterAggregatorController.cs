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

        /// <summary>
        /// Get all clothing filters.
        /// </summary>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Clothing filters DTO.</returns>
        [HttpGet]
        public async Task<ActionResult<ClotheFiltersDTO>> GetAllFilters(CancellationToken cancelletionToken)
        {
            ClotheFiltersDTO? filters = await filterClient.GetAllFiltersAsync(cancelletionToken);
            if (filters == null)
            {
                logger.LogWarning("Failed to get filters");
                return StatusCode(500, "Could not retrieve filters");
            }

            return Ok(filters);
        }
    }
}