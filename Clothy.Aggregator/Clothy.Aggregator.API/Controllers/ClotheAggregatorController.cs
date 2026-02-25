using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.Aggregator.API.Controllers
{
    [ApiController]
    [Route("api/clothe")]
    public class ClotheAggregatorController : ControllerBase
    {
        private IClotheAggregateService aggregatorService;
        private ILogger<ClotheAggregatorController> logger;

        public ClotheAggregatorController(IClotheAggregateService aggregatorService, ILogger<ClotheAggregatorController> logger)
        {
            this.aggregatorService = aggregatorService;
            this.logger = logger;
        }

        /// <summary>
        /// Get detailed aggregated information about a specific clothing item.
        /// </summary>
        /// <param name="slug">Slug of the clothing item.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Full details of the clothing item.</returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<ClotheDetailFullDTO>> GetClotheFullDetail(string slug, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Starting getting detailed aggregated information on clothing with slug: {Slug}", slug);

            ClotheDetailFullDTO? result = await aggregatorService.GetFullClotheDetailAsync(slug, cancelletionToken);
            return Ok(result);
        }
    }
}