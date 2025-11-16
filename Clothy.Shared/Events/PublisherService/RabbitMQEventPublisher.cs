using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;

namespace Clothy.Shared.Events.PublisherService
{
    public class RabbitMQEventPublisher : IEventPublisher
    {
        private IConnectionFactory connectionFactory;
        private IHttpContextAccessor httpContextAccessor;
        private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

        public RabbitMQEventPublisher(IConnectionFactory connectionFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.connectionFactory = connectionFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, string exchangeName, string routingKey) where TEvent : class
        {
            await using var connection = await connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            BasicProperties properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = new Dictionary<string, object?>
                {
                    ["EventType"] = typeof(TEvent).Name,
                    ["CorrelationId"] = GetCorrelationId(@event)
                }
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }

        private string GetCorrelationId<TEvent>(TEvent @event) where TEvent : class
        {
            string? correlationId = httpContextAccessor.HttpContext?.Items[CORRELATION_ID_HEADER]?.ToString();
            if (!string.IsNullOrEmpty(correlationId)) return correlationId;
            
            else return Guid.NewGuid().ToString();
        }
    }
}