using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagController : ControllerBase
    {
        private ITagService tagService;
        private ILogger<TagController> logger;

        public TagController(ITagService tagService, ILogger<TagController> logger)
        {
            this.tagService = tagService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all tags.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of all tags.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching all tags.");
            List<TagReadDTO> tags = await tagService.GetAllAsync(cancellationToken);
            return Ok(tags);
        }

        /// <summary>
        /// Get a tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tag details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching tag with ID: {Id}", id);
            TagReadDTO tag = await tagService.GetByIdAsync(id, cancellationToken);
            
            return Ok(tag);
        }

        /// <summary>
        /// Create a new tag.
        /// </summary>
        /// <param name="tagCreateDTO">Tag creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created tag.</returns>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create(TagCreateDTO tagCreateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating tag with name: {Name}", tagCreateDTO.Name);
            TagReadDTO created = await tagService.CreateAsync(tagCreateDTO, cancellationToken);
            
            logger.LogInformation("Tag created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="tagUpdateDTO">Tag update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated tag.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Update(Guid id, TagUpdateDTO tagUpdateDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating tag with ID: {Id}", id);
            TagReadDTO updated = await tagService.UpdateAsync(id, tagUpdateDTO, cancellationToken);
            
            logger.LogInformation("Tag with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting tag with ID: {Id}", id);
            await tagService.DeleteAsync(id, cancellationToken);
            
            logger.LogInformation("Tag with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
