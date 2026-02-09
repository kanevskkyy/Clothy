using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.BLL.Services.Interfaces;
using Clothy.PaymentService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Clothy.PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private IPaymentServiceFactory paymentServiceFactory;
        private ILogger<PaymentController> logger;

        public PaymentController(IPaymentServiceFactory paymentServiceFactory, ILogger<PaymentController> logger)
        {
            this.paymentServiceFactory = paymentServiceFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Create a Stripe payment for an order.
        /// </summary>
        /// <param name="request">Payment request containing OrderId.</param>
        /// <param name="cancelletionToken">Cancellation token.</param>
        /// <param name="method">Payment method.</param>
        /// <returns>Payment response with PaymentId, URL, and status.</returns>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<CreatePaymentResponseDTO>> CreatePayment([FromBody] CreatePaymentRequestDTO request, [FromQuery] PaymentMethod method = PaymentMethod.Card, CancellationToken cancelletionToken = default)
        {
            logger.LogInformation("Creating {Method} payment for Order {OrderId}", method, request.OrderId);

            IPaymentService paymentService = paymentServiceFactory.GetPaymentService(method);
            CreatePaymentResponseDTO payment = await paymentService.CreatePaymentAsync(request, User, cancelletionToken);

            logger.LogInformation("Payment created. PaymentId={PaymentId}, Method={Method}", payment.PaymentId, method);

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
            using var reader = new StreamReader(Request.Body);
            string? payload = await reader.ReadToEndAsync();
            string? signature = Request.Headers["Stripe-Signature"];

            logger.LogInformation("Received Stripe webhook");

            IPaymentService stripeService = paymentServiceFactory.GetPaymentService(PaymentMethod.Card);
            await stripeService.HandleWebhookAsync(payload, signature!, cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Handle NowPayment webhook events.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Empty result.</returns>
        [HttpPost("webhook/nowpayment")]
        [AllowAnonymous]
        public async Task<IActionResult> NowPaymentWebhook(CancellationToken cancellationToken = default)
        {
            using StreamReader reader = new StreamReader(Request.Body);
            string payload = await reader.ReadToEndAsync();
            string? signature = Request.Headers["x-nowpayments-sig"].FirstOrDefault();

            logger.LogInformation("Received NowPayments webhook");

            IPaymentService cryptoService = paymentServiceFactory.GetPaymentService(PaymentMethod.Crypto);
            await cryptoService.HandleWebhookAsync(payload, signature!, cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Retry a cancelled or failed payment.
        /// </summary>
        /// <param name="paymentId">ID of the previous payment.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>New payment response with PaymentId, URL, and status.</returns>
        [HttpPost("retry/{paymentId}")]
        [Authorize]
        public async Task<ActionResult<CreatePaymentResponseDTO>> RetryPayment(Guid paymentId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrying payment {PaymentId}", paymentId);

            IPaymentService paymentService = paymentServiceFactory.GetPaymentService(PaymentMethod.Card);
            CreatePaymentResponseDTO payment = await paymentService.RetryPaymentAsync(paymentId, User, cancellationToken);

            logger.LogInformation("Retry payment created. OldPaymentId={OldPaymentId}, NewPaymentId={NewPaymentId}", paymentId, payment.PaymentId);

            return Ok(payment);
        }
    }
}
