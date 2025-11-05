using Clothy.Aggregator.Aggregate.DTOs.ClotheItem;
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

        [HttpGet("{clotheId}")]
        public async Task<ActionResult<ClotheDetailFullDTO>> GetClotheFullDetail(Guid clotheId, CancellationToken ct)
        {
            ClotheDetailFullDTO? result = await aggregatorService.GetFullClotheDetailAsync(clotheId, ct);
            return Ok(result);
        }
    }
}