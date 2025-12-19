using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private IPaymentService paymentService;
        private ILogger<PaymentController> logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            this.paymentService = paymentService;
            this.logger = logger;
        }

        /// <summary>
        /// Create a Stripe payment for an order.
        /// </summary>
        /// <param name="request">Payment request containing OrderId.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <returns>Payment response with PaymentId, URL, and status.</returns>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<CreatePaymentResponseDTO>> CreatePayment([FromBody] CreatePaymentRequestDTO request, CancellationToken cancelletionToken)
        {
            logger.LogInformation("Creating payment for Order {OrderId}", request.OrderId);
            CreatePaymentResponseDTO payment = await paymentService.CreatePaymentAsync(request, User, cancelletionToken);

            logger.LogInformation("Payment created. PaymentId={PaymentId}", payment.PaymentId);
            return Ok(payment);
        }

        /// <summary>
        /// Handle incoming Stripe webhook events.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Empty result.</returns>
        [HttpPost("webhook/stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken = default)
        {
            using StreamReader reader = new StreamReader(Request.Body);
            string? payload = await reader.ReadToEndAsync();
            string? stripeSignature = Request.Headers["Stripe-Signature"];

            logger.LogInformation("Received Stripe webhook.");
            await paymentService.HandleWebhookAsync(payload, stripeSignature, cancellationToken);

            return Ok();
        }
    }
}
