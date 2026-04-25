using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
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

[Collection("PaymentService")]
public class NowPaymentsControllerTests
{
    private HttpClient client;
    private PaymentServiceWebApplicationFactory factory;

    private static JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public NowPaymentsControllerTests(PaymentServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        factory.OrderInfoClientMock.Reset();
    }

    [Fact]
    public async Task NowPaymentWebhook_WithInvalidSignature_ShouldNotUpdate()
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        Guid paymentId = await Seed(userId, orderId);

        string payload = JsonSerializer.Serialize(new
        {
            payment_status = "finished",
            order_id = paymentId.ToString()
        });

        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook/nowpayment")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        req.Headers.Add("x-nowpayments-sig", "wrong");

        await client.SendAsync(req);

        using var scope = factory.Services.CreateScope();
        PaymentDbContext db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        PaymentRecordEF? record = await db.PaymentRecords.FindAsync(paymentId);

        record!.Status.Should().Be(PaymentStatus.Pending);
    }

    private async Task<Guid> Seed(Guid userId, Guid orderId)
    {
        using var scope = factory.Services.CreateScope();
        PaymentDbContext db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        PaymentRecordEF record = new PaymentRecordEF
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Price = 100,
            Status = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.Crypto,
            TransactionId = "test"
        };

        db.PaymentRecords.Add(record);
        await db.SaveChangesAsync();

        return record.Id;
    }
}