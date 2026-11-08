using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/payments/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class PaymentAdminController : ControllerBase
    {
        private IPaymentAdminService paymentAdminService;

        public PaymentAdminController(IPaymentAdminService paymentAdminService)
        {
            this.paymentAdminService = paymentAdminService;
        }

        /// <summary>
        /// Payment status of each order (Pending, Paid, Disputed, DisputeWon, DisputeLost).
        /// </summary>
        /// <param name="orderIds">Up to 100 order ids: ?orderIds=...&amp;orderIds=...</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>One record per order that has a payment.</returns>
        [HttpGet("statuses")]
        public async Task<ActionResult<List<OrderPaymentStatusDTO>>> GetStatuses([FromQuery] List<Guid> orderIds, CancellationToken cancellationToken = default)
        {
            List<OrderPaymentStatusDTO> statuses = await paymentAdminService.GetStatusesByOrdersAsync(orderIds.Distinct().ToList(), cancellationToken);
            return Ok(statuses);
        }
    }
}
