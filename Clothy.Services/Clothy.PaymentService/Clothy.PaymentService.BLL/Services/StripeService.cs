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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Services
{
    public class StripeService : IPaymentService
    {
        public PaymentDbContext dbContext;
        private IGetOrderInfoClient orderInfoClient;
        private ILogger<StripeService> logger;
        private PaymentSettings stripeSettings;
        private IUserClaimsExtractor userClaimsExtractor;
        private IPublishEndpoint publishEndpoint;

        public StripeService(PaymentDbContext dbContext, 
            IGetOrderInfoClient orderInfoClient, 
            ILogger<StripeService> logger,
            IOptions<PaymentSettings> stripeSettings,
            IUserClaimsExtractor userClaimsExtractor,
            IPublishEndpoint publishEndpoint)
        {
            this.dbContext = dbContext;
            this.orderInfoClient = orderInfoClient;
            this.logger = logger;
            this.stripeSettings = stripeSettings.Value;
            this.userClaimsExtractor = userClaimsExtractor;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task<CreatePaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO request, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating Stripe payment for Order {OrderId}", request.OrderId);

            GetOrderInfoResponse orderInfoResponse = await orderInfoClient.GetOrderInfoAsync(request.OrderId, cancellationToken);

            if (orderInfoResponse.Status.ToLower() != "awaiting payment") throw new ValidationFailedException($"The order with OrderId: {request.OrderId} has already been paid for!");

            Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);
            if (userId != Guid.Parse(orderInfoResponse.UserId)) throw new ValidationFailedException("You cannot pay for someone else's order!");

            PaymentRecordEF paymentRecord = new PaymentRecordEF
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Parse(orderInfoResponse.OrderId),
                UserId = Guid.Parse(orderInfoResponse.UserId),
                Price = decimal.Parse(orderInfoResponse.Price),
                Status = PaymentStatus.Pending
            };           

            StripeConfiguration.ApiKey = stripeSettings.SecretKey;
            SessionCreateOptions sessionCreateOptions = new SessionCreateOptions()
            {
                Mode = "payment",
                SuccessUrl = stripeSettings.SuccessUrl,
                CancelUrl = stripeSettings.CancelUrl,
                LineItems = new List<SessionLineItemOptions>()
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(paymentRecord.Price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order {paymentRecord.OrderId}"
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string>()
                {
                    { 
                        "payment_id", paymentRecord.Id.ToString() 
                    },
                    { 
                        "order_id", paymentRecord.OrderId.ToString() 
                    },
                    {
                        "user_id", paymentRecord.UserId.ToString() 
                    }
                }
            };

            SessionService sessionService = new SessionService();
            Session session = await sessionService.CreateAsync(sessionCreateOptions, cancellationToken: cancellationToken);

            paymentRecord.TransactionId = session.Id;
            await dbContext.PaymentRecords.AddAsync(paymentRecord, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Stripe checkout session created. PaymentId={PaymentId}, SessionId={SessionId}", paymentRecord.Id, session.Id);

            return new CreatePaymentResponseDTO
            {
                PaymentId = paymentRecord.Id,
                PaymentUrl = session.Url,
                Status = paymentRecord.Status
            };
        }

        public async Task HandleWebhookAsync(string payload, string stripeSignature, CancellationToken cancellationToken)
        {
            Stripe.Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, stripeSettings.WebhookSecret);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook signature verification failed.");
                return;
            }

            logger.LogInformation("Stripe webhook received: {Type}", stripeEvent.Type);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                Session? session = stripeEvent.Data.Object as Session;
                if (session == null)
                {
                    logger.LogWarning("Checkout session is null in webhook.");
                    return;
                }

                if (!session.Metadata.TryGetValue("payment_id", out var paymentIdStr) || !Guid.TryParse(paymentIdStr, out var paymentId))
                {
                    logger.LogWarning("PaymentId not found or invalid in metadata.");
                    return;
                }

                PaymentRecordEF? payment = await dbContext.PaymentRecords.FindAsync(new object[] { paymentId }, cancellationToken);

                if (payment == null)
                {
                    logger.LogWarning("PaymentRecord not found for PaymentId={PaymentId}", paymentId);
                    return;
                }

                if (payment.Status == PaymentStatus.Paid)
                {
                    logger.LogInformation("Payment {PaymentId} already marked as Paid", paymentId);
                    return;
                }

                payment.Status = PaymentStatus.Paid;
                payment.UpdatedAt = DateTime.UtcNow.ToUniversalTime();
                await dbContext.SaveChangesAsync(cancellationToken);

                OrderPaidEvent orderPaidEvent = new OrderPaidEvent()
                {
                    OrderId = payment.OrderId
                };
                await publishEndpoint.Publish(orderPaidEvent, cancellationToken);

                logger.LogInformation("Payment {PaymentId} marked as Paid via webhook", paymentId);
            }
            else
            {
                logger.LogInformation("Ignoring Stripe event type: {Type}", stripeEvent.Type);
            }
        }
    }
}
