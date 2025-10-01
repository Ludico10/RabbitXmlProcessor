using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Config;


//get configs from file
var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

//initialize the logger factory for all classes
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();

    if (config.GetValue("Logging:Console:Enabled", true))
        builder.AddConsole();

    if (config.GetValue("Logging:File:Enabled", false))
    {
        builder.AddFile(options =>
        {
            options = config.GetValue<FileLoggerOptions>("Logging:File:Options") ?? new FileLoggerOptions();
        });
    }

    builder.SetMinimumLevel(LogLevel.Information);
});

//create logger for current class
var logger = loggerFactory.CreateLogger<Program>();

//create cancellation token (exit on Ctrl+C)
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    //initialize rabbitMq
    var rabbitOptions = config.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
    var rabbitCreateTask = RabbitPostman.CreateAsync(rabbitOptions, loggerFactory, cts.Token);

    //initialize facadeclass
    var parser = new ParallelXmlParser(loggerFactory);

    var dir = config.GetValue("InputDirectory", "./input");
    var interval = config.GetValue("MonitoringInterval", 1000);
    var useHash = config.GetValue("useFileHash", false);
    //wait for rabbit
    using var postman = await rabbitCreateTask;

    //start the directory browsing cycle
    await parser.MonitorAsync(dir, interval, useHash, postman, cts.Token);
}
catch (Exception ex)
{
    logger.LogError($"Service error: {ex.Message}");
}
