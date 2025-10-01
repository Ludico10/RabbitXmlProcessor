using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Config;
using Shared.Model;
using System.Text;
using System.Text.Json;

namespace Shared
{
    /// <summary>
    /// Handles sending and receiving JSON messages via RabbitMQ.
    /// </summary>
    public class RabbitPostman : IDisposable
    {
        private readonly ILogger _logger;

        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;

        public RabbitPostman(IChannel channel, RabbitMqOptions options, ILogger logger)
        {
            _logger = logger;

            _channel = channel;
            _options = options;
        }

        /// <summary>
        /// Asynchronously creates and initializes a RabbitPostman instance.
        /// </summary>
        /// <param name="options">RabbitMQ connection and queue options.</param>
        /// <param name="loggerFactory">Factory for creating loggers.</param>
        /// <param name="ct">Cancellation token for async operations.</param>
        /// <returns>A fully initialized RabbitPostman.</returns>
        public static async Task<RabbitPostman> CreateAsync(RabbitMqOptions options, ILoggerFactory loggerFactory, CancellationToken ct = default)
        {
            //initialize connection channel
            var factory = new ConnectionFactory()
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
            var connection = await factory.CreateConnectionAsync(ct);
            var channel = await connection.CreateChannelAsync(null, ct);

            if (channel is null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            //creating an exchanger
            await channel.ExchangeDeclareAsync(
                    exchange: options.Exchange,
                    type: ExchangeType.Direct,  //by exact key match
                    durable: true,
                    autoDelete: false
                    );

            var logger = loggerFactory.CreateLogger<RabbitPostman>();

            return new RabbitPostman(channel, options, logger);
        }

        /// <summary>
        /// Sends a JSON message to the configured RabbitMQ exchange.
        /// </summary>
        /// <param name="json">The JSON string to send.</param>
        /// <param name="ct">Cancellation token for async operations.</param>
        public async Task SendAsync(string json, CancellationToken ct = default)
        {
            var body = Encoding.UTF8.GetBytes(json);
            var props = new BasicProperties
            {
                Persistent = true
            };

            //sending
            await _channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            _logger.LogDebug($"Json sended: {json}.");
        }

        /// <summary>
        /// Subscribes to the configured RabbitMQ queue and processes received messages.
        /// </summary>
        /// <param name="processFuncAsync">Function to handle each received InstrumentStatus.</param>
        /// <param name="ct">Cancellation token to stop receiving messages.</param>
        public async Task ReceiveAsync(Func<InstrumentStatus, CancellationToken, Task> processFuncAsync, CancellationToken ct = default)
        {
            if (_channel is null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            //initialize queue
            await _channel.QueueDeclareAsync(
                queue: _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false
                );

            await _channel.QueueBindAsync(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey
                );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            //subscribe to receive messages
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    _logger.LogDebug($"Json received: {json}.");

                    var data = JsonSerializer.Deserialize<InstrumentStatus>(json);
                    if (data is not null)
                    {
                        //saving to db
                        await processFuncAsync(data, ct);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Json receiving error: {ex.Message}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
                }
            };

            //listen to the channel
            await _channel.BasicConsumeAsync(
                queue: _options.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: ct);

            _logger.LogInformation("Waiting for data started.");

            //indefinitely until the token is received
            await Task.Delay(-1, ct);

            _logger.LogInformation("Request processing has stopped.");
        }

        public void Dispose() => _channel.Dispose();
    }
}
