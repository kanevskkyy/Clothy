using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/clothes")]
    public class ClotheController : ControllerBase
    {
        private IClotheService clotheService;
        private ILogger<ClotheController> logger;

        public ClotheController(IClotheService clotheService, ILogger<ClotheController> logger)
        {
            this.clotheService = clotheService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paged clothes.
        /// </summary>
        /// <param name="parameters">Query parameters for pagination, filtering, sorting.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paged list of clothes.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<ClotheSummaryDTO>>> GetPaged([FromQuery] ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged clothes.");
            PagedList<ClotheSummaryDTO> pagedClothes = await clotheService.GetPagedClotheItemsAsync(parameters, cancellationToken);
            
            return Ok(pagedClothes);
        }

        /// <summary>
        /// Get a single clothe item by Slug with details.
        /// </summary>
        /// <param name="slug">Clothe slug.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Clothe details.</returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<ClotheDetailDTO>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching clothe with Slug: {Slug}", slug);
            ClotheDetailDTO clothe = await clotheService.GetDetailBySlugAsync(slug, cancellationToken);
            
            return Ok(clothe);
        }

        /// <summary>
        /// Create a new clothe item.
        /// </summary>
        /// <param name="clotheCreateDTO">Clothe creation data (supports main photo + additional photos).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created clothe item.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<ClotheDetailDTO>> Create([FromForm] ClotheCreateDTO clotheCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating clothe with name: {Name}", clotheCreateDTO.Name);
            ClotheDetailDTO created = await clotheService.CreateAsync(clotheCreateDTO, cancellationToken);

            logger.LogInformation("Clothe created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }

        /// <summary>
        /// Update an existing clothe item by ID.
        /// </summary>
        /// <param name="id">Clothe ID (GUID).</param>
        /// <param name="clotheUpdateDTO">Update data for the clothe (supports main photo + additional photos).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated clothe item.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<ClotheDetailDTO>> Update(Guid id, [FromForm] ClotheUpdateDTO clotheUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating clothe with ID: {Id}", id);
            ClotheDetailDTO updated = await clotheService.UpdateAsync(id, clotheUpdateDTO, cancellationToken);

            logger.LogInformation("Clothe with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Get minimum and maximum price among all clothes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Object with min and max price.</returns>
        [HttpGet("pricerange")]
        public async Task<ActionResult<PriceRangeDTO>> GetPriceRange(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching price range for clothes.");

            PriceRangeDTO priceRangeDTO = await clotheService.GetMinAndMaxPriceAsync(cancellationToken);
            return Ok(priceRangeDTO);
        }

        /// <summary>
        /// Delete a clothe item by ID.
        /// </summary>
        /// <param name="id">Clothe ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting clothe with ID: {Id}", id);
            await clotheService.DeleteAsync(id, cancellationToken);

            logger.LogInformation("Clothe with ID {Id} deleted.", id);
            return NoContent();
        }

        /// <summary>
        /// Get top 8 most popular clothes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of top 8 popular clothes.</returns>
        [HttpGet("top8")]
        public async Task<ActionResult<List<ClotheSummaryDTO>>> GetTop8MostPopular(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching top 8 most popular clothes.");

            List<ClotheSummaryDTO>? topClothes = await clotheService.GetTop8MostPopularAsync(cancellationToken);

            return Ok(topClothes);
        }
    }
}
