using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/clothes-stock")]
    public class ClothesStockController : ControllerBase
    {
        private IClothesStockService clothesStockService;
        private ILogger<ClothesStockController> logger;

        public ClothesStockController(IClothesStockService clothesStockService, ILogger<ClothesStockController> logger)
        {
            this.clothesStockService = clothesStockService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paged clothes stock.
        /// </summary>
        /// <param name="parameters">Query parameters for pagination, filtering, sorting.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Paged list of clothes stock.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<ClothesStockReadDTO>>> GetPaged([FromQuery] ClothesStockSpecificationParameters parameters, CancellationToken ct)
        {
            logger.LogInformation("Fetching paged clothes stock.");
            PagedList<ClothesStockReadDTO> pagedStock = await clothesStockService.GetPagedClothesStockAsync(parameters, ct);

            return Ok(pagedStock);
        }

        /// <summary>
        /// Get a single clothes stock item by ID with details.
        /// </summary>
        /// <param name="id">Clothes stock ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Clothes stock details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothesStockReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching clothes stock with ID: {Id}", id);
            ClothesStockReadDTO stock = await clothesStockService.GetByIdWithDetailsAsync(id, ct);

            return Ok(stock);
        }

        /// <summary>
        /// Create a new clothes stock item.
        /// </summary>
        /// <param name="dto">Clothes stock creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created clothes stock item.</returns>
        [HttpPost]
        public async Task<ActionResult<ClothesStockReadDTO>> Create([FromBody] ClothesStockCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating clothes stock for ClotheId: {ClotheId}", dto.ClotheId);
            ClothesStockReadDTO created = await clothesStockService.CreateAsync(dto, ct);

            logger.LogInformation("Clothes stock created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing clothes stock item by ID.
        /// </summary>
        /// <param name="id">Clothes stock ID (GUID).</param>
        /// <param name="dto">Update data for the clothes stock.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated clothes stock item.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClothesStockReadDTO>> Update(Guid id, [FromBody] ClothesStockUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating clothes stock with ID: {Id}", id);
            ClothesStockReadDTO updated = await clothesStockService.UpdateAsync(id, dto, ct);

            logger.LogInformation("Clothes stock with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a clothes stock item by ID.
        /// </summary>
        /// <param name="id">Clothes stock ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting clothes stock with ID: {Id}", id);
            await clothesStockService.DeleteAsync(id, ct);

            logger.LogInformation("Clothes stock with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
