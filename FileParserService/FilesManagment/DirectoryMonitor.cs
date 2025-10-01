using FileParserService.FilesManagment;
using Microsoft.Extensions.Logging;

/// <summary>
/// Periodically monitors a directory for XML files and triggers processing callbacks.
/// Uses a thread-safe file cache to detect new or modified files.
/// </summary>
public class DirectoryMonitor(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DirectoryMonitor>();

    private readonly FileCache _cache = new(loggerFactory);

    /// <summary>
    /// Continuously scans the specified directory at the given interval.
    /// </summary>
    /// <param name="dir">The directory to monitor for XML files.</param>
    /// <param name="interval">Interval in milliseconds between scans.</param>
    /// <param name="useHash">Whether to use file hash to detect changes.</param>
    /// <param name="fileProcessing">Async callback executed for each file detected or changed.</param>
    /// <param name="ct">Cancellation token to stop monitoring.</param>
    /// <returns>A Task representing the asynchronous monitoring operation.</returns>
    public async Task WatchAsync(
        string dir,
        int interval,
        bool useHash,
        Func<string, Task> fileProcessing,
        CancellationToken ct = default)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));

        var hashMessage = useHash ? "High precision mode" : "";
        _logger.LogInformation($"Parsing started. Directory: {dir}, interval: {interval}ms. {hashMessage}");

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                var currentFiles = Directory.GetFiles(dir, "*.xml").ToHashSet();
                var tasks = new List<Task>();
                foreach (var file in currentFiles)
                {
                    tasks.Append(CheckFileAsync(file, useHash, fileProcessing, ct));
                }

                _cache.Clean(currentFiles);

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Parser error: {ex.Message}");
            }
        }

        _logger.LogInformation("Parsing stopped.");
    }

    /// <summary>
    /// Checks a single file for changes and triggers processing if it is new or modified.
    /// </summary>
    /// <param name="file">Path to the file to check.</param>
    /// <param name="useHash">Whether to use file hash to detect changes.</param>
    /// <param name="fileProcessing">Async callback to process the file if changed.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A Task representing the asynchronous file check operation.</returns>
    public async Task CheckFileAsync(string file, bool useHash, Func<string, Task> fileProcessing, CancellationToken ct = default)
    {
        try
        {
            //track changes in files
            if (!File.Exists(file))
            {
                _logger.LogWarning($"File {file} was not found before processing.");
                return;
            }

            var meta = new FileMeta(file, useHash);
            if (!_cache.HasChanged(file, meta))
            {
                _logger.LogDebug($"File {file} was not changed.");
                return;
            }

            //and process
            await fileProcessing(file);

            _cache.Update(file, meta);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during check of file {file} : {ex.Message}");
        }
    }
}
