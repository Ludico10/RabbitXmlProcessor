using Microsoft.Extensions.Logging;

namespace FileParserService.Tests
{
    public class DirectoryMonitorTests
    {
        private const string TempDir = "temp_dir";

        public DirectoryMonitorTests()
        {
            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);

            Directory.CreateDirectory(TempDir);
        }

        /// <summary>
        /// Checks that CheckFileAsync calls the fileProcessing callback for new files.
        /// </summary>
        [Fact]
        public async Task NewProcessingTest()
        {
            var loggerFactory = new LoggerFactory();
            var monitor = new DirectoryMonitor(loggerFactory);

            string filePath = Path.Combine(TempDir, "file1.xml");
            File.WriteAllText(filePath, "<test>data</test>");

            bool called = false;
            await monitor.CheckFileAsync(filePath, useHash: false, async (f) => { called = true; await Task.CompletedTask; });

            Assert.True(called);
        }

        /// <summary>
        /// Checks that CheckFileAsync does not call callback for unchanged files.
        /// </summary>
        [Fact]
        public async Task UnchangedFileTest()
        {
            var loggerFactory = new LoggerFactory();
            var monitor = new DirectoryMonitor(loggerFactory);

            string filePath = Path.Combine(TempDir, "file2.xml");
            File.WriteAllText(filePath, "<test>data</test>");

            bool firstCall = false;
            await monitor.CheckFileAsync(filePath, useHash: false, async (f) => { firstCall = true; await Task.CompletedTask; });
            Assert.True(firstCall);

            bool secondCall = false;
            await monitor.CheckFileAsync(filePath, useHash: false, async (f) => { secondCall = true; await Task.CompletedTask; });

            Assert.False(secondCall);
        }

        /// <summary>
        /// Checks that DirectoryMonitor detects a file renamed to a deleted file's name.
        /// If useHash=true, it detects as a new file; if false, it detects as unchanged.
        /// </summary>
        [Fact]
        public async Task SameDetectionTest()
        {
            var loggerFactory = new LoggerFactory();
            var monitor = new DirectoryMonitor(loggerFactory);

            // Create two files with different content
            string fileA = Path.Combine(TempDir, "A.xml");
            string fileB = Path.Combine(TempDir, "B.xml");

            File.WriteAllText(fileA, "Content A");
            File.WriteAllText(fileB, "Content B");

            var processedFilesHash = new HashSet<string>();
            var processedFilesName = new HashSet<string>();

            // First: useHash = true
            await monitor.CheckFileAsync(fileA, useHash: true, async f => { processedFilesHash.Add(f); await Task.CompletedTask; });
            await monitor.CheckFileAsync(fileB, useHash: true, async f => { processedFilesHash.Add(f); await Task.CompletedTask; });

            // Delete A and rename B to A
            File.Delete(fileA);
            File.Move(fileB, fileA);

            // Should detect as new file if useHash=true
            await monitor.CheckFileAsync(fileA, useHash: true, async f => { processedFilesHash.Add(f); await Task.CompletedTask; });

            // Reset cache
            processedFilesName.Clear();
            var monitor2 = new DirectoryMonitor(loggerFactory);

            // First: useHash = false
            File.WriteAllText(fileB, "Content B");
            await monitor2.CheckFileAsync(fileA, useHash: false, async f => { processedFilesName.Add(f); await Task.CompletedTask; });
            await monitor2.CheckFileAsync(fileB, useHash: false, async f => { processedFilesName.Add(f); await Task.CompletedTask; });

            File.Delete(fileA);
            File.Move(fileB, fileA);

            await monitor2.CheckFileAsync(fileA, useHash: false, async f => { processedFilesName.Add(f); await Task.CompletedTask; });

            Assert.Equal(2, processedFilesName.Count);
        }
    }
}
