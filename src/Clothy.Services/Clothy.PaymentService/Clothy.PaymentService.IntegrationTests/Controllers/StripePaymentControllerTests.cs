using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clothy.PaymentService.BLL.DTOs;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Clothy.PaymentService.IntegrationTests.Controllers;

public class StripePaymentControllerTests : IClassFixture<PaymentServiceWebApplicationFactory>
{
    private HttpClient client;
    private PaymentServiceWebApplicationFactory factory;

    private static JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public StripePaymentControllerTests(PaymentServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        factory.OrderInfoClientMock.Reset();
    }

    [Fact]
    public async Task CreatePayment_Stripe_WithoutAuth_ShouldReturnUnauthorized()
    {
        CreatePaymentRequestDTO dto = new() { OrderId = Guid.NewGuid() };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/payments/create?method=Card", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePayment_Stripe_WhenOrderAlreadyPaid_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.OrderInfoClientMock.SetupOrderAlreadyPaid(orderId, userId);

        CreatePaymentRequestDTO dto = new() { OrderId = orderId };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/payments/create?method=Card", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePayment_Stripe_WhenOrderBelongsToAnotherUser_ShouldReturnForbidden()
    {
        Guid userId = Guid.NewGuid();
        Guid anotherUserId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.OrderInfoClientMock.SetupOrderAwaitingPayment(orderId, anotherUserId);

        CreatePaymentRequestDTO dto = new() { OrderId = orderId };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/payments/create?method=Card", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task RetryPayment_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PostAsync($"/api/payments/retry/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RetryPayment_WhenPaymentNotFound_ShouldReturnNotFound()
    {
        Guid userId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.PostAsync($"/api/payments/retry/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RetryPayment_WhenAlreadyPaid_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        Guid paymentId = await SeedPaymentRecord(userId, orderId, PaymentStatus.Paid);

        factory.OrderInfoClientMock.SetupOrderAwaitingPayment(orderId, userId);

        HttpResponseMessage response = await client.PostAsync($"/api/payments/retry/{paymentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RetryPayment_WhenBelongsToAnotherUser_ShouldReturnForbidden()
    {
        Guid realOwner = Guid.NewGuid();
        Guid attacker = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();

        Guid paymentId = await SeedPaymentRecord(realOwner, orderId, PaymentStatus.Pending);

        client.AddAuthorizationHeader(attacker);
        factory.OrderInfoClientMock.SetupOrderAwaitingPayment(orderId, realOwner);

        HttpResponseMessage response = await client.PostAsync($"/api/payments/retry/{paymentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task StripeWebhook_WithInvalidSignature_ShouldNotUpdatePayment()
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        Guid paymentId = await SeedPaymentRecord(userId, orderId, PaymentStatus.Pending);

        string payload = JsonSerializer.Serialize(new
        {
            type = "checkout.session.completed",
            data = new
            {
                @object = new
                {
                    metadata = new
                    {
                        payment_id = paymentId.ToString()
                    }
                }
            }
        });

        HttpRequestMessage request = new(HttpMethod.Post, "/api/payments/webhook/stripe")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Stripe-Signature", "invalid");

        await client.SendAsync(request);

        using IServiceScope scope = factory.Services.CreateScope();
        PaymentDbContext db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        PaymentRecordEF record = await db.PaymentRecords.FindAsync(paymentId);

        record.Status.Should().Be(PaymentStatus.Pending);
    }

    private async Task<Guid> SeedPaymentRecord(Guid userId, Guid orderId, PaymentStatus status)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PaymentDbContext db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        PaymentRecordEF record = new PaymentRecordEF
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Price = 199,
            Status = status,
            PaymentMethod = PaymentMethod.Card,
            TransactionId = "test"
        };

        db.PaymentRecords.Add(record);
        await db.SaveChangesAsync();

        return record.Id;
    }
}