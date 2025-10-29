using Clothy.Aggregator.DTOs.ClotheItem;
using Clothy.Aggregator.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.Aggregator.Controllers
{
    [ApiController]
    [Route("api/clothe")]
    public class ClotheAggregatorController : ControllerBase
    {
        private ClotheAggregatorService aggregatorService;
        private ILogger<ClotheAggregatorController> logger;

        public ClotheAggregatorController(ClotheAggregatorService aggregatorService, ILogger<ClotheAggregatorController> logger)
        {
            this.aggregatorService = aggregatorService;
            this.logger = logger;
        }

        [HttpGet("{clotheId}")]
        public async Task<ActionResult<ClotheDetailFullDTO>> GetClotheFullDetail(Guid clotheId, CancellationToken ct)
        {
            ClotheDetailFullDTO? result = await aggregatorService.GetClotheFullDetailAsync(clotheId, ct);
            if (result == null)
            {
                logger.LogWarning("Clothe with id {Id} not found", clotheId);
                return NotFound();
            }

            return Ok(result);
        }
    }
}
