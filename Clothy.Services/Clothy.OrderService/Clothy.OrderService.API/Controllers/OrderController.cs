using System.Security.Claims;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
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
        /// <param name="filter">Filtering and sorting parameters.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Paged list of orders.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedList<OrderReadDTO>>> GetPaged([FromQuery] OrderFilterDTO filter, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching paged orders.");
            PagedList<OrderReadDTO> pagedOrders = await orderService.GetPagedAsync(filter, cancellationToken: cancelletionToken);

            return Ok(pagedOrders);
        }

        /// <summary>
        /// Get paged orders for the current user with filtering and sorting.
        /// </summary>
        /// <param name="filter">Filtering and sorting parameters.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Paged list of orders for the current user.</returns>
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<PagedList<OrderReadDTO>>> GetPagedMyOrders([FromQuery] OrderFilterDTO filter, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching paged orders for current user.");
            PagedList<OrderReadDTO> pagedOrders = await orderService.GetPagedAsync(filter, User, cancelletionToken);

            return Ok(pagedOrders);
        }

        /// <summary>
        /// Get a single order by ID with details.
        /// </summary>
        /// <param name="id">ID of the order.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Order details.</returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<OrderDetailDTO>> GetById(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Fetching order with ID: {Id}", id);
            OrderDetailDTO? order = await orderService.GetByIdAsync(id, User, cancelletionToken);

            return Ok(order);
        }

        /// <summary>
        /// Create a new order.
        /// </summary>
        /// <param name="dto">Order creation data.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Created order details.</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderReadDTO>> Create([FromBody] OrderCreateDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Creating order for user.");
            OrderDetailDTO created = await orderService.CreateAsync(dto, User, cancellationToken: cancelletionToken);

            logger.LogInformation("Order created with ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update status of an existing order.
        /// </summary>
        /// <param name="id">ID of the order to update.</param>
        /// <param name="dto">Status update data.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Updated order details.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<ActionResult<OrderDetailDTO>> UpdateStatus(Guid id, [FromBody] OrderUpdateStatusDTO dto, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Updating status of order ID: {Id} to StatusId: {StatusId}", id, dto.Status);
            OrderDetailDTO? updated = await orderService.UpdateStatusAsync(id, dto, cancelletionToken);

            logger.LogInformation("Order ID {Id} status updated.", id);
            return Ok(updated);
        }

        /// <summary>
        /// Delete an order by ID.
        /// </summary>
        /// <param name="id">ID of the order to delete.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Deleting order with ID: {Id}", id);
            await orderService.DeleteAsync(id, cancelletionToken);

            logger.LogInformation("Order ID {Id} deleted.", id);
            return NoContent();
        }
    }
}