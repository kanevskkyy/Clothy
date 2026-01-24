using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/collections")]
    public class CollectionController : ControllerBase
    {
        private ICollectionService collectionService;
        private ILogger<CollectionController> logger;

        public CollectionController(ICollectionService collectionService, ILogger<CollectionController> logger)
        {
            this.collectionService = collectionService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all collections.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of all collections.</returns>
        [HttpGet]
        public async Task<ActionResult<List<CollectionReadDTO>>> GetAll(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching all collections.");
            List<CollectionReadDTO> collections = await collectionService.GetAllAsync(cancellationToken);
            
            return Ok(collections);
        }

        /// <summary>
        /// Get a specific collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CollectionReadDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching collection with ID: {Id}", id);
            CollectionReadDTO collection = await collectionService.GetByIdAsync(id, cancellationToken);

            return Ok(collection);
        }

        /// <summary>
        /// Create a new collection.
        /// </summary>
        /// <param name="collectionCreateDTO">Collection creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created collection.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<CollectionReadDTO>> Create([FromBody] CollectionCreateDTO collectionCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating collection with name: {Name}", collectionCreateDTO.Name);
            CollectionReadDTO created = await collectionService.CreateAsync(collectionCreateDTO, cancellationToken);
            
            logger.LogInformation("Collection created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="collectionUpdateDTO">Update data for the collection.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated collection.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<CollectionReadDTO>> Update(Guid id, [FromBody] CollectionUpdateDTO collectionUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating collection with ID: {Id}", id);
            CollectionReadDTO updated = await collectionService.UpdateAsync(id, collectionUpdateDTO, cancellationToken);
            
            logger.LogInformation("Collection with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting collection with ID: {Id}", id);
            await collectionService.DeleteAsync(id, cancellationToken);
            
            logger.LogInformation("Collection with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
