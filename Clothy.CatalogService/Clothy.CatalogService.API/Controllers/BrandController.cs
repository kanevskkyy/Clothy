using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandController : ControllerBase
    {
        private IBrandService brandService;
        private ILogger<BrandController> logger;

        public BrandController(IBrandService brandService, ILogger<BrandController> logger)
        {
            this.brandService = brandService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all brands.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all brands.</returns>
        [HttpGet]
        public async Task<ActionResult<List<BrandReadDTO>>> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all brands.");
            List<BrandReadDTO> brands = await brandService.GetAllAsync(ct);
            
            return Ok(brands);
        }

        /// <summary>
        /// Get a brand by its ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Brand details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BrandReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching brand with ID: {Id}", id);
            BrandReadDTO brand = await brandService.GetByIdAsync(id, ct);
            
            return Ok(brand);
        }

        /// <summary>
        /// Create a new brand.
        /// </summary>
        /// <param name="dto">Brand creation data (supports image upload).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created brand.</returns>
        [HttpPost]
        public async Task<ActionResult<BrandReadDTO>> Create([FromForm] BrandCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating brand with name: {Name}", dto.Name);
            BrandReadDTO created = await brandService.CreateAsync(dto, ct);

            logger.LogInformation("Brand created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing brand by ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="dto">Update data for the brand (supports image upload).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated brand.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BrandReadDTO>> Update(Guid id, [FromForm] BrandUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating brand with ID: {Id}", id);
            BrandReadDTO updated = await brandService.UpdateAsync(id, dto, ct);

            logger.LogInformation("Brand with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a brand by its ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting brand with ID: {Id}", id);
            await brandService.DeleteAsync(id, ct);

            logger.LogInformation("Brand with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
