using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/sizes")]
    public class SizeController : ControllerBase
    {
        private ISizeService sizeService;
        private ILogger<SizeController> logger;

        public SizeController(ISizeService sizeService, ILogger<SizeController> logger)
        {
            this.sizeService = sizeService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all sizes.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all sizes.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SizeReadDTO>>> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all sizes.");
            var sizes = await sizeService.GetAllAsync(ct);
            return Ok(sizes);
        }

        /// <summary>
        /// Get a size by its ID.
        /// </summary>
        /// <param name="id">Size ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Size details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SizeReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching size with ID: {Id}", id);
            SizeReadDTO size = await sizeService.GetByIdAsync(id, ct);
            return Ok(size);
        }

        /// <summary>
        /// Create a new size.
        /// </summary>
        /// <param name="dto">Size creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created size.</returns>
        [HttpPost]
        public async Task<ActionResult<SizeReadDTO>> Create([FromBody] SizeCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating size with name: {Name}", dto.Name);
            SizeReadDTO createdSize = await sizeService.CreateAsync(dto, ct);
            logger.LogInformation("Size created with ID: {Id}", createdSize.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdSize.Id }, createdSize);
        }

        /// <summary>
        /// Update an existing size by its ID.
        /// </summary>
        /// <param name="id">Size ID (GUID).</param>
        /// <param name="dto">Size update data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated size.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SizeReadDTO>> Update(Guid id, [FromBody] SizeUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating size with ID: {Id}", id);
            SizeReadDTO updatedSize = await sizeService.UpdateAsync(id, dto, ct);
            
            logger.LogInformation("Size with ID {Id} updated.", id);
            return Ok(updatedSize);
        }

        /// <summary>
        /// Delete a size by its ID.
        /// </summary>
        /// <param name="id">Size ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting size with ID: {Id}", id);
            await sizeService.DeleteAsync(id, ct);
            
            logger.LogInformation("Size with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
