using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/settlements")]
    public class SettlementController : ControllerBase
    {
        private ISettlementService settlementService;
        private ILogger<SettlementController> logger;

        public SettlementController(ISettlementService settlementService, ILogger<SettlementController> logger)
        {
            this.settlementService = settlementService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paginated list of settlements with optional filtering and sorting.
        /// </summary>
        /// <param name="settlementFilterDTO">Filter parameters for settlements.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of settlements.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<SettlementReadDTO>>> GetPagedAsync([FromQuery] SettlementFilterDTO settlementFilterDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching paged settlements. Page: {PageNumber}, PageSize: {PageSize}", settlementFilterDTO.PageNumber, settlementFilterDTO.PageSize);

            PagedList<SettlementReadDTO> pagedList = await settlementService.GetPagedAsync(settlementFilterDTO, cancellationToken);
            return Ok(pagedList);
        }


        /// <summary>
        /// Get a specific settlement by its ID.
        /// </summary>
        /// <param name="id">Region ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Settlement details.</returns>
        [HttpGet("{id:guid}", Name = "GetSettlementById")]
        public async Task<ActionResult<SettlementReadDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching settlement with ID: {Id}", id);
            SettlementReadDTO settlementReadDTO = await settlementService.GetByIdAsync(id, cancellationToken);
            
            return Ok(settlementReadDTO);
        }

        /// <summary>
        /// Create a new settlement.
        /// </summary>
        /// <param name="settlementCreateDTO">Settlement creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created settlement.</returns>
        [HttpPost]
        public async Task<ActionResult<SettlementReadDTO>> CreateAsync([FromBody] SettlementCreateDTO settlementCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating region with name: {Name}", settlementCreateDTO.Name);

            SettlementReadDTO created = await settlementService.CreateAsync(settlementCreateDTO, cancellationToken);
            logger.LogInformation("Settlement created with ID: {Id}", created.Id);

            return CreatedAtRoute("GetSettlementById", new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing settlement by its ID.
        /// </summary>
        /// <param name="id">Region ID (GUID).</param>
        /// <param name="settlementUpdateDTO">Update data for the settlement.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated settlement.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SettlementReadDTO>> UpdateAsync(Guid id, [FromBody] SettlementUpdateDTO settlementUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating settlement with ID: {Id}", id);

            SettlementReadDTO updated = await settlementService.UpdateAsync(id, settlementUpdateDTO, cancellationToken);

            logger.LogInformation("Settlement with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a settlement by its ID.
        /// </summary>
        /// <param name="id">Settlement ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting settlement with ID: {Id}", id);
            await settlementService.DeleteAsync(id, cancellationToken);

            logger.LogInformation("Settlement with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
