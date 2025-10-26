using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/colors")]
    public class ColorController : ControllerBase
    {
        private IColorService colorService;
        private ILogger<ColorController> logger;

        public ColorController(IColorService colorService, ILogger<ColorController> logger)
        {
            this.colorService = colorService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all colors.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all colors.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ColorReadDTO>>> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all colors.");
            List<ColorReadDTO> colors = await colorService.GetAllAsync(ct);
            
            return Ok(colors);
        }

        /// <summary>
        /// Get all colors with stock information.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of colors including stock info.</returns>
        [HttpGet("with-stock")]
        public async Task<ActionResult<IEnumerable<ColorWithCountDTO>>> GetAllWithStock(CancellationToken ct)
        {
            logger.LogInformation("Fetching all colors with stock info.");
            List<ColorWithCountDTO> colors = await colorService.GetAllWithCountAsync(ct);
            
            return Ok(colors);
        }

        /// <summary>
        /// Get a specific color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Color details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ColorReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching color with ID: {Id}", id);
            ColorReadDTO color = await colorService.GetByIdAsync(id, ct);
            
            return Ok(color);
        }

        /// <summary>
        /// Create a new color.
        /// </summary>
        /// <param name="dto">Color creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created color.</returns>
        [HttpPost]
        public async Task<ActionResult<ColorReadDTO>> Create([FromBody] ColorCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating color with hex code: {HexCode}", dto.HexCode);
            ColorReadDTO created = await colorService.CreateAsync(dto, ct);
            
            logger.LogInformation("Color created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="dto">Color update data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated color.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ColorReadDTO>> Update(Guid id, [FromBody] ColorUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating color with ID: {Id}", id);
            ColorReadDTO updated = await colorService.UpdateAsync(id, dto, ct);
            
            logger.LogInformation("Color with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting color with ID: {Id}", id);
            await colorService.DeleteAsync(id, ct);
            
            logger.LogInformation("Color with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
