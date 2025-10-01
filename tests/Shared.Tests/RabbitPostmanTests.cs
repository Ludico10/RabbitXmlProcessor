using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Shared.Config;
using Shared.Model;
using System.Text.Json;

namespace Shared.Tests
{
    public class RabbitPostmanTests
    {
        private readonly RabbitMqOptions _options = new()
        {
            Host = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            Exchange = "test-exchange",
            Queue = "test-queue",
            RoutingKey = "test-key"
        };

        /// <summary>
        /// Verifies that SendAsync correctly sends a JSON message through the RabbitMQ channel.
        /// </summary>
        [Fact]
        public async Task SendTest()
        {
            var mockChannel = new Mock<IChannel>();
            var mockLoggerFactory = new Mock<ILogger>();

            var postman = new RabbitPostman(mockChannel.Object, _options, mockLoggerFactory.Object);

            var device = new DeviceStatus() { ModuleCategoryID = "XXX", ModuleState = ModuleState.Run };
            string json = JsonSerializer.Serialize(new InstrumentStatus { PackageID = "123", Devices = [device] });

            await postman.SendAsync(json);
        }
    }
}
