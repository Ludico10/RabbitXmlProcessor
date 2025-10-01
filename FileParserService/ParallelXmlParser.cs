using FileParserService.DataProcessing;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;


/// <summary>
/// Facade for XML parsing, combining monitoring of a directory, processing of XML files,
/// enriching instrument/device statuses, and caching file metadata.
/// </summary>
internal class ParallelXmlParser(ILoggerFactory loggerFactory)
{
    private readonly DirectoryMonitor _monitor = new(loggerFactory);
    private readonly FileProcessor _processor = new(loggerFactory);
    private readonly ModuleStateHelper _helper = new(loggerFactory);

    /// <summary>
    /// Starts monitoring the specified directory for XML files and processes them.
    /// </summary>
    /// <param name="dir">The directory to monitor for XML files.</param>
    /// <param name="interval">Interval in milliseconds between directory scans.</param>
    /// <param name="useHash">Whether to use file hash for detecting changes.</param>
    /// <param name="postman">RabbitPostman instance for sending processed JSON messages.</param>
    /// <param name="ct">Optional cancellation token to stop monitoring.</param>
    /// <returns>A Task representing the asynchronous monitoring operation.</returns>
    public Task MonitorAsync(string dir, int interval, bool useHash, RabbitPostman postman, CancellationToken ct = default)
    {
        //file actions between parsing and sending
        Task extraTask(InstrumentStatus status) => Task.Run(() => _helper.Enrich(status), ct);

        return _monitor.WatchAsync(
            dir,
            interval,
            useHash,
            file => _processor.ProcessAsync(file, extraTask, postman, ct),
            ct);
    }
}
