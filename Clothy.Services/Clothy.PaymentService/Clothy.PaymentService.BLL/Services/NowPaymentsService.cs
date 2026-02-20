using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.BLL.Services.Interfaces;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Services
{
    public class NowPaymentsService : IPaymentService
    {
        public PaymentMethod PaymentMethod => PaymentMethod.Crypto;

        private PaymentDbContext dbContext;
        private IGetOrderInfoClient orderInfoClient;
        private ILogger<NowPaymentsService> logger;
        private CryptoSettings nowPaymentsSettings;
        private IPublishEndpoint publishEndpoint;
        private IUserClaimsExtractor userClaimsExtractor;
        private HttpClient httpClient;

        public NowPaymentsService(
            PaymentDbContext dbContext,
            IGetOrderInfoClient orderInfoClient,
            ILogger<NowPaymentsService> logger,
            IOptions<CryptoSettings> nowPaymentsSettings,
            IUserClaimsExtractor userClaimsExtractor,
            IHttpClientFactory httpClientFactory,
            IPublishEndpoint publishEndpoint)
        {
            this.dbContext = dbContext;
            this.publishEndpoint = publishEndpoint;
            this.orderInfoClient = orderInfoClient;
            this.logger = logger;
            this.nowPaymentsSettings = nowPaymentsSettings.Value;
            this.userClaimsExtractor = userClaimsExtractor;
            httpClient = httpClientFactory.CreateClient("NowPayments");

            httpClient.DefaultRequestHeaders.Add(this.nowPaymentsSettings.ApiKeyHeader, this.nowPaymentsSettings.ApiKey);
        }

        public async Task<CreatePaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO request, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating NowPayments crypto payment for Order {OrderId}", request.OrderId);

            GetOrderInfoResponse orderInfoResponse = await orderInfoClient.GetOrderInfoAsync(request.OrderId, cancellationToken);

            if (orderInfoResponse.Status != OrderStatusGrpc.AwaitingPayment) throw new ValidationFailedException($"The order with OrderId: {request.OrderId} has already been paid for!");

            Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);
            if (userId != Guid.Parse(orderInfoResponse.UserId)) throw new ForbiddenException("You cannot pay for someone else's order!");

            PaymentRecordEF paymentRecord = new PaymentRecordEF
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Parse(orderInfoResponse.OrderId),
                UserId = Guid.Parse(orderInfoResponse.UserId),
                Price = decimal.Parse(orderInfoResponse.Price),
                Status = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Crypto
            };

            object paymentRequest = new
            {
                price_amount = paymentRecord.Price,
                price_currency = nowPaymentsSettings.PriceCurrency,
                ipn_callback_url = nowPaymentsSettings.CallbackURL,
                order_id = paymentRecord.Id.ToString(),
                order_description = $"Payment for order {paymentRecord.OrderId}",
                success_url = nowPaymentsSettings.SuccessURL,
                cancel_url = $"{nowPaymentsSettings.CancelURL}?paymentId={paymentRecord.Id}"
            };

            string jsonContent = JsonSerializer.Serialize(paymentRequest);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(nowPaymentsSettings.BaseURL, content, cancellationToken);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                string errorContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("NowPayments API error: {StatusCode}, {Content}", httpResponseMessage.StatusCode, errorContent);

                throw new Exception($"Failed to create NowPayments payment: {httpResponseMessage.StatusCode}");
            }

            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument doc = JsonDocument.Parse(responseContent);

            string? paymentId = doc.RootElement.GetProperty("id").GetString();
            string? paymentUrl = doc.RootElement.GetProperty("invoice_url").GetString();

            paymentRecord.TransactionId = paymentId;
            await dbContext.PaymentRecords.AddAsync(paymentRecord, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("NowPayments payment created. PaymentId={PaymentId}, NowPaymentsId={NowPaymentsId}", paymentRecord.Id, paymentId);

            return new CreatePaymentResponseDTO
            {
                PaymentId = paymentRecord.Id,
                PaymentUrl = paymentUrl,
                Status = paymentRecord.Status
            };
        }

        public async Task<CreatePaymentResponseDTO> RetryPaymentAsync(Guid paymentId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrying payment {PaymentId}", paymentId);

            PaymentRecordEF? oldPayment = await dbContext.PaymentRecords.FindAsync(new object[] { paymentId }, cancellationToken);

            if (oldPayment == null) throw new NotFoundException($"Payment with ID {paymentId} not found!");

            Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);
            if (userId != oldPayment.UserId) throw new ForbiddenException("You cannot retry someone else's payment!");

            if (oldPayment.Status == PaymentStatus.Paid) throw new ValidationFailedException("Cannot retry already paid payment!");

            GetOrderInfoResponse orderInfoResponse = await orderInfoClient.GetOrderInfoAsync(oldPayment.OrderId, cancellationToken);

            if (orderInfoResponse.Status != OrderStatusGrpc.AwaitingPayment) throw new ValidationFailedException($"The order {oldPayment.OrderId} is no longer available for payment!");

            PaymentRecordEF newPaymentRecord = new PaymentRecordEF
            {
                Id = Guid.NewGuid(),
                OrderId = oldPayment.OrderId,
                UserId = oldPayment.UserId,
                Price = decimal.Parse(orderInfoResponse.Price),
                Status = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Crypto
            };

            object paymentRequest = new
            {
                price_amount = newPaymentRecord.Price,
                price_currency = nowPaymentsSettings.PriceCurrency,
                ipn_callback_url = nowPaymentsSettings.CallbackURL,
                order_id = newPaymentRecord.Id.ToString(),
                order_description = $"Payment for order {newPaymentRecord.OrderId}",
                success_url = nowPaymentsSettings.SuccessURL,
                cancel_url = $"{nowPaymentsSettings.CancelURL}?paymentId={newPaymentRecord.Id}"
            };

            string jsonContent = JsonSerializer.Serialize(paymentRequest);
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(nowPaymentsSettings.BaseURL, content, cancellationToken);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                string errorContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("NowPayments API error: {StatusCode}, {Content}", httpResponseMessage.StatusCode, errorContent);

                throw new Exception($"Failed to create NowPayments payment: {httpResponseMessage.StatusCode}");
            }

            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument doc = JsonDocument.Parse(responseContent);

            string? nowPaymentId = doc.RootElement.GetProperty("id").GetString();
            string? paymentUrl = doc.RootElement.GetProperty("invoice_url").GetString();

            newPaymentRecord.TransactionId = nowPaymentId;
            await dbContext.PaymentRecords.AddAsync(newPaymentRecord, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Retry payment created. OldPaymentId={OldPaymentId}, NewPaymentId={NewPaymentId}, NowPaymentsId={NowPaymentsId}",
                paymentId, newPaymentRecord.Id, nowPaymentId);

            return new CreatePaymentResponseDTO
            {
                PaymentId = newPaymentRecord.Id,
                PaymentUrl = paymentUrl,
                Status = newPaymentRecord.Status
            };
        }

        public async Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing NowPayments webhook");

            if (!VerifyWebhookSignature(payload, signature))
            {
                logger.LogWarning("Invalid NowPayments webhook signature");
                return;
            }

            using JsonDocument doc = JsonDocument.Parse(payload);
            JsonElement root = doc.RootElement;

            string? paymentStatus = root.TryGetProperty("payment_status", out JsonElement statusElement) ? statusElement.GetString() : null;
            string? orderId = root.TryGetProperty("order_id", out JsonElement orderElement) ? orderElement.GetString() : null;

            logger.LogInformation("Webhook payload: OrderId={OrderId}, Status={Status}", orderId, paymentStatus);

            if (paymentStatus?.ToLower() != "finished")
            {
                logger.LogInformation("Ignoring webhook - payment not finished yet. Status: {Status}", paymentStatus);
                return;
            }

            if (string.IsNullOrEmpty(orderId) || !Guid.TryParse(orderId, out Guid paymentId))
            {
                logger.LogWarning("Invalid order_id in webhook payload");
                return;
            }

            PaymentRecordEF? paymentRecord = await dbContext.PaymentRecords.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (paymentRecord == null)
            {
                logger.LogWarning("Payment record not found for ID: {PaymentId}", paymentId);
                return;
            }

            if (paymentRecord.Status != PaymentStatus.Paid)
            {
                logger.LogInformation("Updating payment to Succeeded. PaymentId={PaymentId}, OldStatus={OldStatus}",
                    paymentId, paymentRecord.Status);

                paymentRecord.Status = PaymentStatus.Paid;
                paymentRecord.UpdatedAt = DateTime.UtcNow.ToUniversalTime();

                await dbContext.SaveChangesAsync(cancellationToken);

                OrderPaidEvent orderPaidEvent = new OrderPaidEvent()
                {
                    OrderId = paymentRecord.OrderId
                };
                await publishEndpoint.Publish(orderPaidEvent, cancellationToken);

                logger.LogInformation("Payment {PaymentId} marked as Paid via webhook", paymentId);
            }
            else
            {
                logger.LogInformation("Payment already marked as Succeeded. PaymentId={PaymentId}", paymentId);
            }
        }

        private bool VerifyWebhookSignature(string payload, string signature)
        {
            if (string.IsNullOrEmpty(signature))
            {
                logger.LogWarning("Webhook signature is missing");
                return false;
            }

            try
            {
                using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(nowPaymentsSettings?.WebhookSecret!));

                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                bool isValid = signature.ToLower() == computedSignature;
                if (!isValid) logger.LogWarning("Signature mismatch");

                return isValid;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying webhook signature");
                return false;
            }
        }
    }
}