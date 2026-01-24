using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of all colors.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ColorReadDTO>>> GetAll(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching all colors.");
            List<ColorReadDTO> colors = await colorService.GetAllAsync(cancellationToken);
            
            return Ok(colors);
        }

        /// <summary>
        /// Get a specific color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Color details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ColorReadDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching color with ID: {Id}", id);
            ColorReadDTO color = await colorService.GetByIdAsync(id, cancellationToken);
            
            return Ok(color);
        }

        /// <summary>
        /// Create a new color.
        /// </summary>
        /// <param name="colorCreateDTO">Color creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created color.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<ColorReadDTO>> Create([FromBody] ColorCreateDTO colorCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating color with hex code: {HexCode}", colorCreateDTO.HexCode);
            ColorReadDTO created = await colorService.CreateAsync(colorCreateDTO, cancellationToken);
            
            logger.LogInformation("Color created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="colorUpdateDTO">Color update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated color.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<ColorReadDTO>> Update(Guid id, [FromBody] ColorUpdateDTO colorUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating color with ID: {Id}", id);
            ColorReadDTO updated = await colorService.UpdateAsync(id, colorUpdateDTO, cancellationToken);
            
            logger.LogInformation("Color with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a color by its ID.
        /// </summary>
        /// <param name="id">Color ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting color with ID: {Id}", id);
            await colorService.DeleteAsync(id, cancellationToken);
            
            logger.LogInformation("Color with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
