using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.Helpers;
using Clothy.CatalogService.Domain.QueryParameters;
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
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Paged list of clothes.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<ClotheSummaryDTO>>> GetPaged([FromQuery] ClotheItemSpecificationParameters parameters, CancellationToken ct)
        {
            logger.LogInformation("Fetching paged clothes.");
            PagedList<ClotheSummaryDTO> pagedClothes = await clotheService.GetPagedClotheItemsAsync(parameters, ct);
            
            return Ok(pagedClothes);
        }

        /// <summary>
        /// Get a single clothe item by ID with details.
        /// </summary>
        /// <param name="id">Clothe ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Clothe details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClotheDetailDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching clothe with ID: {Id}", id);
            ClotheDetailDTO clothe = await clotheService.GetDetailByIdAsync(id, ct);
            
            return Ok(clothe);
        }

        /// <summary>
        /// Create a new clothe item.
        /// </summary>
        /// <param name="dto">Clothe creation data (supports main photo + additional photos).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created clothe item.</returns>
        [HttpPost]
        public async Task<ActionResult<ClotheDetailDTO>> Create([FromForm] ClotheCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating clothe with name: {Name}", dto.Name);
            ClotheDetailDTO created = await clotheService.CreateAsync(dto, ct);

            logger.LogInformation("Clothe created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing clothe item by ID.
        /// </summary>
        /// <param name="id">Clothe ID (GUID).</param>
        /// <param name="dto">Update data for the clothe (supports main photo + additional photos).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated clothe item.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClotheDetailDTO>> Update(Guid id, [FromForm] ClotheUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating clothe with ID: {Id}", id);
            ClotheDetailDTO updated = await clotheService.UpdateAsync(id, dto, ct);

            logger.LogInformation("Clothe with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a clothe item by ID.
        /// </summary>
        /// <param name="id">Clothe ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting clothe with ID: {Id}", id);
            await clotheService.DeleteAsync(id, ct);

            logger.LogInformation("Clothe with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
