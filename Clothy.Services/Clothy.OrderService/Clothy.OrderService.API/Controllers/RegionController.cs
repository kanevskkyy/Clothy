using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/regions")]
    public class RegionController : ControllerBase
    {
        private IRegionService regionService;
        private ILogger<RegionController> logger;

        public RegionController(IRegionService regionService, ILogger<RegionController> logger)
        {
            this.regionService = regionService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paginated list of regions with optional filtering and sorting.
        /// </summary>
        /// <param name="regionFilterDTO">Filter parameters for regions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of regions.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<RegionReadDTO>>> GetPagedAsync([FromQuery] RegionFilterDTO regionFilterDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged regions. Page: {PageNumber}, PageSize: {PageSize}",regionFilterDTO.PageNumber, regionFilterDTO.PageSize);

            PagedList<RegionReadDTO> pagedList = await regionService.GetPagedAsync(regionFilterDTO, cancellationToken);
            return Ok(pagedList);
        }

        /// <summary>
        /// Get a specific region by its ID.
        /// </summary>
        /// <param name="id">Region ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Region details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RegionReadDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching region with ID: {Id}", id);
            RegionReadDTO regionReadDTO = await regionService.GetByIdAsync(id, cancellationToken);
            
            return Ok(regionReadDTO);
        }

        /// <summary>
        /// Create a new region.
        /// </summary>
        /// <param name="regionCreateDTO">Region creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created region.</returns>
        [HttpPost]
        public async Task<ActionResult<RegionReadDTO>> Create([FromBody] RegionCreateDTO regionCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating region with name: {Name}", regionCreateDTO.Name);
            RegionReadDTO created = await regionService.CreateAsync(regionCreateDTO, cancellationToken);
            logger.LogInformation("Region created with ID: {Id}", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing region by its ID.
        /// </summary>
        /// <param name="id">Region ID (GUID).</param>
        /// <param name="regionUpdateDTO">Update data for the region.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated region.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RegionReadDTO>> Update(Guid id, [FromBody] RegionUpdateDTO regionUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating region with ID: {Id}", id);
            RegionReadDTO updated = await regionService.UpdateAsync(id, regionUpdateDTO, cancellationToken);
            
            logger.LogInformation("Region with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a region by its ID.
        /// </summary>
        /// <param name="id">Region ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting region with ID: {Id}", id);
            await regionService.DeleteAsync(id, cancellationToken);
            
            logger.LogInformation("Region with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}