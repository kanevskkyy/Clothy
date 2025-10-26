using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.Interfaces;
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
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all collections.</returns>
        [HttpGet]
        public async Task<ActionResult<List<CollectionReadDTO>>> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all collections.");
            List<CollectionReadDTO> collections = await collectionService.GetAllAsync(ct);
            
            return Ok(collections);
        }

        /// <summary>
        /// Get all collections with item count.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of collections including the count of items in each collection.</returns>
        [HttpGet("with-item-count")]
        public async Task<ActionResult<IEnumerable<CollectionWithCountDTO>>> GetAllWithItemCount(CancellationToken ct)
        {
            logger.LogInformation("Fetching all collections with item count.");
            List<CollectionWithCountDTO> collections = await collectionService.GetAllWithCountAsync(ct);
            
            return Ok(collections);
        }

        /// <summary>
        /// Get a specific collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Collection details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CollectionReadDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching collection with ID: {Id}", id);
            CollectionReadDTO collection = await collectionService.GetByIdAsync(id, ct);

            return Ok(collection);
        }

        /// <summary>
        /// Create a new collection.
        /// </summary>
        /// <param name="dto">Collection creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created collection.</returns>
        [HttpPost]
        public async Task<ActionResult<CollectionReadDTO>> Create([FromBody] CollectionCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating collection with name: {Name}", dto.Name);
            CollectionReadDTO created = await collectionService.CreateAsync(dto, ct);
            
            logger.LogInformation("Collection created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="dto">Update data for the collection.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated collection.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CollectionReadDTO>> Update(Guid id, [FromBody] CollectionUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating collection with ID: {Id}", id);
            CollectionReadDTO updated = await collectionService.UpdateAsync(id, dto, ct);
            
            logger.LogInformation("Collection with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a collection by its ID.
        /// </summary>
        /// <param name="id">Collection ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting collection with ID: {Id}", id);
            await collectionService.DeleteAsync(id, ct);
            
            logger.LogInformation("Collection with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
