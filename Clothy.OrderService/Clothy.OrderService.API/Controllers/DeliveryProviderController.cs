using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/delivery-providers")]
    public class DeliveryProviderController : ControllerBase
    {
        private IDeliveryProviderService deliveryProviderService;
        private ILogger<DeliveryProviderController> logger;

        public DeliveryProviderController(IDeliveryProviderService deliveryProviderService, ILogger<DeliveryProviderController> logger)
        {
            this.deliveryProviderService = deliveryProviderService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all delivery providers.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DeliveryProviderReadDTO>>> GetAll(CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching all delivery providers.");
            List<DeliveryProviderReadDTO> providers = await deliveryProviderService.GetAllAsync(cancelletionToken);
            
            return Ok(providers);
        }

        /// <summary>
        /// Get delivery provider by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DeliveryProviderReadDTO>> GetById(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching delivery provider with ID: {Id}", id);
            DeliveryProviderReadDTO provider = await deliveryProviderService.GetByIdAsync(id, cancelletionToken);
            
            return Ok(provider);
        }

        /// <summary>
        /// Create a new delivery provider.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DeliveryProviderReadDTO>> Create([FromForm] DeliveryProviderCreateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Creating delivery provider with name: {Name}", dto.Name);
            DeliveryProviderReadDTO created = await deliveryProviderService.CreateAsync(dto, cancelletionToken);
            
            logger.LogInformation("Delivery provider created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new 
            { 
                id = created.Id 
            }, created);
        }

        /// <summary>
        /// Update an existing delivery provider.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DeliveryProviderReadDTO>> Update(Guid id, [FromForm] DeliveryProviderUpdateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Updating delivery provider with ID: {Id}", id);
            DeliveryProviderReadDTO updated = await deliveryProviderService.UpdateAsync(id, dto, cancelletionToken);
            
            logger.LogInformation("Delivery provider with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete a delivery provider by ID.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Deleting delivery provider with ID: {Id}", id);
            await deliveryProviderService.DeleteAsync(id, cancelletionToken);
            
            logger.LogInformation("Delivery provider with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
