using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace Clothy.Shared.Events.ConsumerService
{
    public abstract class BaseEventListenerService<TEvent> : BackgroundService
    {
        protected IConnectionFactory connectionFactory;
        protected ILogger logger;
        protected IServiceScopeFactory serviceScopeFactory;
        private IConnection? connection;
        private IChannel? channel;
        protected abstract string ExchangeName { get; }
        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }

        protected BaseEventListenerService(IConnectionFactory connectionFactory, ILogger logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("{ServiceName} is starting", GetType().Name);

            try
            {
                connection = await connectionFactory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.BasicQosAsync(0, 1, false, stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    arguments: null,
                    cancellationToken: stoppingToken);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += OnMessageReceived;

                await channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                logger.LogInformation("{ServiceName} is running", GetType().Name);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error in {ServiceName}", GetType().Name);
                throw;
            }
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs ea)
        {
            string correlationId = ExtractCorrelationId(ea);

            try
            {
                logger.LogInformation("Received message, CorrelationId: {CorrelationId}", correlationId);

                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<TEvent>(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (message == null)
                {
                    logger.LogWarning("Received null message, rejecting. CorrelationId: {CorrelationId}", correlationId);
                    await channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                    await handler.HandleAsync(message);
                }

                await channel!.BasicAckAsync(ea.DeliveryTag, false);
                logger.LogInformation("Successfully processed message, CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message, CorrelationId: {CorrelationId}", correlationId);
                await channel!.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        }

        private string ExtractCorrelationId(BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties.Headers?.ContainsKey("CorrelationId") == true)
            {
                var value = ea.BasicProperties.Headers["CorrelationId"];
                return value switch
                {
                    byte[] bytes => Encoding.UTF8.GetString(bytes),
                    string str => str,
                    _ => value?.ToString() ?? "Unknown"
                };
            }
            return "Unknown";
        }
    }
}
