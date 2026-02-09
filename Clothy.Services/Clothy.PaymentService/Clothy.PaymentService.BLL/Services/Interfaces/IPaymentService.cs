using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO request, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken);
        Task HandleWebhookAsync(string payload, string stripeSignature, CancellationToken cancellationToken);
        Task<CreatePaymentResponseDTO> RetryPaymentAsync(Guid paymentId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken);

        PaymentMethod PaymentMethod { get; }
    }
}
