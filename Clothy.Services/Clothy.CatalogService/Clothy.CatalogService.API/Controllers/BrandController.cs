using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of all brands.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<BrandReadDTO>>> GetAll(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching all brands.");
            List<BrandReadDTO> brands = await brandService.GetAllAsync(cancellationToken);
            
            return Ok(brands);
        }

        /// <summary>
        /// Get a brand by its ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Brand details.</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<BrandReadDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching brand with ID: {Id}", id);
            BrandReadDTO brand = await brandService.GetByIdAsync(id, cancellationToken);
            
            return Ok(brand);
        }

        /// <summary>
        /// Create a new brand.
        /// </summary>
        /// <param name="brandCreateDTO">Brand creation data (supports image upload).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created brand.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<BrandReadDTO>> Create([FromBody] BrandCreateDTO brandCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating brand with name: {Name}", brandCreateDTO.Name);
            BrandReadDTO created = await brandService.CreateAsync(brandCreateDTO, cancellationToken);

            logger.LogInformation("Brand created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing brand by ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="brandUpdateDTO">Update data for the brand (supports image upload).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated brand.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<BrandReadDTO>> Update(Guid id, [FromBody] BrandUpdateDTO brandUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating brand with ID: {Id}", id);
            BrandReadDTO updated = await brandService.UpdateAsync(id, brandUpdateDTO, cancellationToken);

            logger.LogInformation("Brand with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a brand by its ID.
        /// </summary>
        /// <param name="id">Brand ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting brand with ID: {Id}", id);
            await brandService.DeleteAsync(id, cancellationToken);

            logger.LogInformation("Brand with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
