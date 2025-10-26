using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.DTOs.FilterDTOs;
using Clothy.OrderService.BLL.Helpers;
using Clothy.OrderService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CityController : ControllerBase
    {
        private ICityService cityService;
        private ILogger<CityController> logger;

        public CityController(ICityService cityService, ILogger<CityController> logger)
        {
            this.cityService = cityService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paginated list of cities with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">Filter parameters for cities.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Paginated list of cities.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<CityReadDTO>>> GetPaged([FromQuery] CityFilterDTO filter, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching paged cities. Page: {PageNumber}, PageSize: {PageSize}", filter.PageNumber, filter.PageSize);
            PagedList<CityReadDTO> cities = await cityService.GetPagedAsync(filter, cancelletionToken);
            
            return Ok(cities);
        }

        /// <summary>
        /// Get a specific city by its ID.
        /// </summary>
        /// <param name="id">City ID (GUID).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>City details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CityReadDTO>> GetById(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching city with ID: {Id}", id);
            CityReadDTO city = await cityService.GetByIdAsync(id, cancelletionToken);
            
            return Ok(city);
        }

        /// <summary>
        /// Create a new city.
        /// </summary>
        /// <param name="dto">City creation data.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Created city.</returns>
        [HttpPost]
        public async Task<ActionResult<CityReadDTO>> Create([FromBody] CityCreateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Creating city with name: {Name}", dto.Name);
            CityReadDTO created = await cityService.CreateAsync(dto, cancelletionToken);
            
            logger.LogInformation("City created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new 
            { 
                id = created.Id 
            }, created);
        }

        /// <summary>
        /// Update an existing city by its ID.
        /// </summary>
        /// <param name="id">City ID (GUID).</param>
        /// <param name="dto">Update data for the city.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Updated city.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CityReadDTO>> Update(Guid id, [FromBody] CityUpdateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Updating city with ID: {Id}", id);
            CityReadDTO updated = await cityService.UpdateAsync(id, dto, cancelletionToken);
            
            logger.LogInformation("City with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a city by its ID.
        /// </summary>
        /// <param name="id">City ID (GUID).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Deleting city with ID: {Id}", id);
            await cityService.DeleteAsync(id, cancelletionToken);
            
            logger.LogInformation("City with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
