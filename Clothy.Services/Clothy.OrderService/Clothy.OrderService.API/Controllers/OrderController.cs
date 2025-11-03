using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private IOrderService orderService;
        private ILogger<OrderController> logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }

        /// <summary>
        /// Get paged orders with filtering and sorting.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedList<OrderReadDTO>>> GetPaged([FromQuery] OrderFilterDTO filter, CancellationToken ct)
        {
            logger.LogInformation("Fetching paged orders.");
            PagedList<OrderReadDTO> pagedOrders = await orderService.GetPagedAsync(filter, ct);
            
            return Ok(pagedOrders);
        }

        /// <summary>
        /// Get a single order by ID with details.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDetailDTO>> GetById(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching order with ID: {Id}", id);
            OrderDetailDTO? order = await orderService.GetByIdAsync(id, ct);
            
            return Ok(order);
        }

        /// <summary>
        /// Create a new order.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderReadDTO>> Create([FromBody] OrderCreateDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Creating order for user: {FirstName} {LastName}", dto.UserFirstName, dto.UserLastName);
            OrderDetailDTO created = await orderService.CreateAsync(dto, ct);
            
            logger.LogInformation("Order created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update status of an existing order.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<OrderDetailDTO>> UpdateStatus(Guid id, [FromBody] OrderUpdateStatusDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Updating status of order ID: {Id} to StatusId: {StatusId}", id, dto.StatusId);
            OrderDetailDTO? updated = await orderService.UpdateStatusAsync(id, dto, ct);
            
            logger.LogInformation("Order ID {Id} status updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete an order by ID.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Deleting order with ID: {Id}", id);
            await orderService.DeleteAsync(id, ct);
            
            logger.LogInformation("Order ID {Id} deleted.", id);
            return NoContent();
        }
    }
}
