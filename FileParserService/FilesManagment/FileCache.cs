using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FileParserService.FilesManagment;

/// <summary>
/// Thread-safe cache for tracking file metadata and detecting changes.
/// </summary>
internal class FileCache(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<FileCache>();

    private readonly ConcurrentDictionary<string, FileMeta> _cache = new();

    /// <summary>
    /// Determines whether the file has changed compared to the cached metadata.
    /// Logs when new or modified files are detected.
    /// </summary>
    /// <param name="file">The file path to check.</param>
    /// <param name="newMeta">The new metadata for the file.</param>
    /// <returns>True if the file is new or changed; otherwise false.</returns>
    public bool HasChanged(string file, FileMeta newMeta)
    {
        if (_cache.TryGetValue(file, out var oldMeta))
        {
            if (newMeta.Equals(oldMeta))
                return false;

            _logger.LogInformation($"Changes detected in file {file}.");
            return true;
        }

        _logger.LogInformation($"Detected new file {file}.");
        return true;
    }

    /// <summary>
    /// Updates or adds the metadata entry for a file in the cache.
    /// </summary>
    /// <param name="file">The file path.</param>
    /// <param name="meta">The file metadata.</param>
    public void Update(string file, FileMeta meta) => _cache[file] = meta;

    /// <summary>
    /// Removes cached entries for files that no longer exist in the directory.
    /// </summary>
    /// <param name="currentFiles">The set of files currently present in the directory.</param>
    public void Clean(HashSet<string> currentFiles)
    {
        foreach (var cachedFile in _cache.Keys)
        {
            if (!currentFiles.Contains(cachedFile))
            {
                if (_cache.TryRemove(cachedFile, out _))
                    _logger.LogDebug($"File {cachedFile} was removed from cache.");
                else
                    _logger.LogWarning($"Failed to remove file {cachedFile} from cache.");
            }
        }
    }
}
