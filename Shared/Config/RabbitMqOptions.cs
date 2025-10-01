namespace Shared.Config
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "";
        public string RoutingKey { get; set; } = "default";
        public string Queue { get; set; } = "default-queue";
    }
}
