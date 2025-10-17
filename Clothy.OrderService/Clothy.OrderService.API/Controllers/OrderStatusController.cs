using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/order-statuses")]
    public class OrderStatusController : ControllerBase
    {
        private IOrderStatusService orderStatusService;
        private ILogger<OrderStatusController> logger;

        public OrderStatusController(IOrderStatusService orderStatusService, ILogger<OrderStatusController> logger)
        {
            this.orderStatusService = orderStatusService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all order statuses.
        /// </summary>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>List of order statuses.</returns>
        [HttpGet]
        public async Task<ActionResult<List<OrderStatusReadDTO>>> GetAll(CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching all order statuses.");
            List<OrderStatusReadDTO> statuses = await orderStatusService.GetAllAsync(cancelletionToken);
            
            return Ok(statuses);
        }

        /// <summary>
        /// Get a single order status by ID.
        /// </summary>
        /// <param name="id">Order status ID (GUID).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Order status details.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderStatusReadDTO>> GetById(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching order status with ID: {Id}", id);
            OrderStatusReadDTO status = await orderStatusService.GetByIdAsync(id, cancelletionToken);
            
            return Ok(status);
        }

        /// <summary>
        /// Create a new order status.
        /// </summary>
        /// <param name="dto">Order status creation data (supports icon file).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Created order status.</returns>
        [HttpPost]
        public async Task<ActionResult<OrderStatusReadDTO>> Create([FromForm] OrderStatusCreateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Creating order status with name: {Name}", dto.Name);
            OrderStatusReadDTO created = await orderStatusService.CreateAsync(dto, cancelletionToken);
            
            logger.LogInformation("Order status created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing order status by ID.
        /// </summary>
        /// <param name="id">Order status ID (GUID).</param>
        /// <param name="dto">Update data for the order status (supports icon file).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Updated order status.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrderStatusReadDTO>> Update(Guid id, [FromForm] OrderStatusUpdateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Updating order status with ID: {Id}", id);
            OrderStatusReadDTO updated = await orderStatusService.UpdateAsync(id, dto, cancelletionToken);
            
            logger.LogInformation("Order status with ID {Id} updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete an order status by ID.
        /// </summary>
        /// <param name="id">Order status ID (GUID).</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Deleting order status with ID: {Id}", id);
            await orderStatusService.DeleteAsync(id, cancelletionToken);
            
            logger.LogInformation("Order status with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
