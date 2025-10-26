using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.Interfaces;
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
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all materials.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all materials.");
            List<MaterialReadDTO> materials = await materialService.GetAllAsync(ct);
            
            return Ok(materials);
        }

        /// <summary>
        /// Get all materials with clothe count.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of materials including the number of clothes using each material.</returns>
        [HttpGet("with-clothe-count")]
        public async Task<IActionResult> GetAllWithClotheCount(CancellationToken ct)
        {
            logger.LogInformation("Fetching all materials with clothe count.");
            List<MaterialWithCountDTO> materials = await materialService.GetAllWithCountAsync(ct);
            
            return Ok(materials);
        }

        /// <summary>
        /// Get a material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Material details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching material with ID: {Id}", id);
            MaterialReadDTO material = await materialService.GetByIdAsync(id, ct);
            
            return Ok(material);
        }

        /// <summary>
        /// Create a new material.
        /// </summary>
        /// <param name="dto">Material creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created material.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaterialCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating material with name: {Name}", dto.Name);
            MaterialReadDTO material = await materialService.CreateAsync(dto, ct);

            logger.LogInformation("Material created with ID: {Id}", material.Id);
            return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
        }

        /// <summary>
        /// Update an existing material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="dto">Material update data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated material.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MaterialUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating material with ID: {Id}", id);
            MaterialReadDTO material = await materialService.UpdateAsync(id, dto, ct);
            
            logger.LogInformation("Material with ID {Id} updated.", id);
            return Ok(material);
        }

        /// <summary>
        /// Delete a material by its ID.
        /// </summary>
        /// <param name="id">Material ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting material with ID: {Id}", id);
            await materialService.DeleteAsync(id, ct);
            
            logger.LogInformation("Material with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
