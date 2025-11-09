using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/pickup-points")]
    public class PickupPointController : ControllerBase
    {
        private IPickupPointService pickupPointService;
        private ILogger<PickupPointController> logger;

        public PickupPointController(IPickupPointService pickupPointService, ILogger<PickupPointController> logger)
        {
            this.pickupPointService = pickupPointService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paginated list of pickup points with optional filtering and sorting.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedList<PickupPointReadDTO>>> GetPagedAsync([FromQuery] PickupPointFilterDTO pickupPointFilterDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged pickup points. Page: {PageNumber}, PageSize: {PageSize}", pickupPointFilterDTO.PageNumber, pickupPointFilterDTO.PageSize);

            PagedList<PickupPointReadDTO> pagedList = await pickupPointService.GetPagedAsync(pickupPointFilterDTO, cancellationToken);
            return Ok(pagedList);
        }

        /// <summary>
        /// Get a specific pickup point by its ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PickupPointReadDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching pickup point with ID: {Id}", id);
            PickupPointReadDTO pickupPoint = await pickupPointService.GetByIdAsync(id, cancellationToken);
            
            return Ok(pickupPoint);
        }

        /// <summary>
        /// Create a new pickup point.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PickupPointReadDTO>> Create([FromBody] PickupPointCreateDTO pickupPointCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating pickup point at address: {Address}", pickupPointCreateDTO.Address);
            PickupPointReadDTO created = await pickupPointService.CreateAsync(pickupPointCreateDTO, cancellationToken);

            logger.LogInformation("Pickup point created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing pickup point by its ID.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PickupPointReadDTO>> Update(Guid id, [FromBody] PickupPointUpdateDTO pickupPointUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating pickup point with ID: {Id}", id);
            PickupPointReadDTO updated = await pickupPointService.UpdateAsync(id, pickupPointUpdateDTO, cancellationToken);

            logger.LogInformation("Pickup point with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a pickup point by its ID.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting pickup point with ID: {Id}", id);
            await pickupPointService.DeleteAsync(id, cancellationToken);

            logger.LogInformation("Pickup point with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
