using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/clothing-types")]
    public class ClothingTypeController : ControllerBase
    {
        private IClothingTypeService clothingTypeService;
        private ILogger<ClothingTypeController> logger;

        public ClothingTypeController(IClothingTypeService clothingTypeService, ILogger<ClothingTypeController> logger)
        {
            this.clothingTypeService = clothingTypeService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all clothing types.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all clothing types.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClothingTypeReadDTO>>> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all clothing types.");
            List<ClothingTypeReadDTO> clothingTypes = await clothingTypeService.GetAllAsync(ct);

            return Ok(clothingTypes);
        }

        /// <summary>
        /// Get a specific clothing type by ID.
        /// </summary>
        /// <param name="id">ClothingType ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Clothing type details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothingTypeReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching clothing type with ID: {Id}", id);
            ClothingTypeReadDTO clothingType = await clothingTypeService.GetByIdAsync(id, ct);

            return Ok(clothingType);
        }

        /// <summary>
        /// Create a new clothing type.
        /// </summary>
        /// <param name="dto">ClothingType creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created clothing type.</returns>
        [HttpPost]
        public async Task<ActionResult<ClothingTypeReadDTO>> Create([FromBody] ClothingTypeCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating clothing type with name: {Name}", dto.Name);
            ClothingTypeReadDTO created = await clothingTypeService.CreateAsync(dto, ct);

            logger.LogInformation("Clothing type created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing clothing type by ID.
        /// </summary>
        /// <param name="id">ClothingType ID (GUID).</param>
        /// <param name="dto">ClothingType update data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated clothing type.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClothingTypeReadDTO>> Update(Guid id, [FromBody] ClothingTypeUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating clothing type with ID: {Id}", id);
            ClothingTypeReadDTO updated = await clothingTypeService.UpdateAsync(id, dto, ct);

            logger.LogInformation("Clothing type with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a clothing type by ID.
        /// </summary>
        /// <param name="id">ClothingType ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting clothing type with ID: {Id}", id);
            await clothingTypeService.DeleteAsync(id, ct);

            logger.LogInformation("Clothing type with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
