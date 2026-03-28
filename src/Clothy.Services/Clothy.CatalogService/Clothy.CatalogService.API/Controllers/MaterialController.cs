using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/materials")]
    public class MaterialController : ControllerBase
    {
        private IMaterialService materialService;
        private ILogger<MaterialController> logger;

        public MaterialController(IMaterialService materialService, ILogger<MaterialController> logger)
        {
            this.materialService = materialService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all materials.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of all materials.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching all materials.");
            List<MaterialReadDTO> materials = await materialService.GetAllAsync(cancellationToken);
            
            return Ok(materials);
        }

        /// <summary>
        /// Get a material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Material details.</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching material with ID: {Id}", id);
            MaterialReadDTO material = await materialService.GetByIdAsync(id, cancellationToken);
            
            return Ok(material);
        }

        /// <summary>
        /// Create a new material.
        /// </summary>
        /// <param name="materialCreateDTO">Material creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created material.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create([FromBody] MaterialCreateDTO materialCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating material with name: {Name}", materialCreateDTO.Name);
            MaterialReadDTO material = await materialService.CreateAsync(materialCreateDTO, cancellationToken);

            logger.LogInformation("Material created with ID: {Id}", material.Id);
            return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
        }

        /// <summary>
        /// Update an existing material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="materialUpdateDTO">Material update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated material.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MaterialUpdateDTO materialUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating material with ID: {Id}", id);
            MaterialReadDTO material = await materialService.UpdateAsync(id, materialUpdateDTO, cancellationToken);
            
            logger.LogInformation("Material with ID {Id} updated.", id);
            return Ok(material);
        }

        /// <summary>
        /// Delete a material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]

        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting material with ID: {Id}", id);
            await materialService.DeleteAsync(id, cancellationToken);
            
            logger.LogInformation("Material with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
