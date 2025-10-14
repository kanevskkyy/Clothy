using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.BLL.Interfaces;
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
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of all tags.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            logger.LogInformation("Fetching all tags.");
            List<TagReadDTO> tags = await tagService.GetAllAsync(ct);
            return Ok(tags);
        }

        /// <summary>
        /// Get all tags with clothe item count.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of tags with item count.</returns>
        [HttpGet("with-item-count")]
        public async Task<IActionResult> GetAllWithItemCount(CancellationToken ct)
        {
            logger.LogInformation("Fetching all tags with clothe count.");
            List<TagWithCountDTO> tags = await tagService.GetAllWithCountAsync(ct);
            return Ok(tags);
        }

        /// <summary>
        /// Get a tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tag details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching tag with ID: {Id}", id);
            TagReadDTO tag = await tagService.GetByIdAsync(id, ct);
            
            return Ok(tag);
        }

        /// <summary>
        /// Create a new tag.
        /// </summary>
        /// <param name="dto">Tag creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created tag.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(TagCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating tag with name: {Name}", dto.Name);
            TagReadDTO created = await tagService.CreateAsync(dto, ct);
            
            logger.LogInformation("Tag created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="dto">Tag update data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Updated tag.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, TagUpdateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating tag with ID: {Id}", id);
            TagReadDTO updated = await tagService.UpdateAsync(id, dto, ct);
            
            logger.LogInformation("Tag with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a tag by its ID.
        /// </summary>
        /// <param name="id">Tag ID (GUID).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting tag with ID: {Id}", id);
            await tagService.DeleteAsync(id, ct);
            
            logger.LogInformation("Tag with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
