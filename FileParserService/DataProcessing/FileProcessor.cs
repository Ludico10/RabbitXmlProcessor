using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;
using System.Text.Json;

namespace FileParserService.DataProcessing
{
    /// <summary>
    /// Handles file processing: deserializing XML files, enriching module states,
    /// and sending processed data to RabbitMQ. Integrates optional additional processing tasks.
    /// </summary>
    internal class FileProcessor(ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<FileProcessor>();

        /// <summary>
        /// Processes a single XML file: deserializes it, executes extra tasks (e.g., changing module states),
        /// serializes the result to JSON, and sends it using RabbitPostman.
        /// </summary>
        /// <param name="file">The path to the XML file to process.</param>
        /// <param name="extraTasks">A function that performs additional processing on the deserialized InstrumentStatus.</param>
        /// <param name="postman">The RabbitPostman instance used to send the JSON message.</param>
        /// <param name="ct">Optional cancellation token to cancel the operation.</param>
        /// <returns>A Task representing the asynchronous file processing operation.</returns>
        public async Task ProcessAsync(string file, Func<InstrumentStatus, Task> extraTasks, RabbitPostman postman, CancellationToken ct = default)
        {
            var root = await XmlParser.DeserializeAsync<InstrumentStatus>(file, ct);
            if (root is not null)
            {
                await extraTasks(root);
                string json = JsonSerializer.Serialize(root);
                await postman.SendAsync(json, ct);
            }
        }
    }
}
