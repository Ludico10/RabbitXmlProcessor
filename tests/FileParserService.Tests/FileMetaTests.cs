using FileParserService.FilesManagment;

namespace FileParserService.Tests
{
    public class FileMetaTests
    {
        private const string TempFile = "temp_test_file.txt";

        /// <summary>
        /// Checks that FileMeta correctly reads file size and last write time.
        /// </summary>
        [Fact]
        public void FileMetadataTest()
        {
            File.WriteAllText(TempFile, "Hello World");

            var meta = new FileMeta(TempFile, useHash: false);

            Assert.Equal(new FileInfo(TempFile).Length, meta.Size);
            Assert.Equal(new FileInfo(TempFile).LastWriteTimeUtc, meta.LastWrite);

            File.Delete(TempFile);
        }

        /// <summary>
        /// Checks that FileMeta computes hash when requested.
        /// </summary>
        [Fact]
        public void ComputeHashTest()
        {
            File.WriteAllText(TempFile, "Hello World");

            var meta = new FileMeta(TempFile, useHash: true);

            Assert.NotNull(meta.Hash);
            Assert.Equal(FileMeta.ComputeHash(TempFile), meta.Hash);

            File.Delete(TempFile);
        }

        /// <summary>
        /// Checks that two FileMeta instances with same content are equal.
        /// </summary>
        [Fact]
        public void SameFileTest()
        {
            File.WriteAllText(TempFile, "Hello World");

            var meta1 = new FileMeta(TempFile, useHash: true);
            var meta2 = new FileMeta(TempFile, useHash: true);

            Assert.True(meta1.Equals(meta2));

            File.Delete(TempFile);
        }

        /// <summary>
        /// Checks that Equals returns false if file size or content differs.
        /// </summary>
        [Fact]
        public void DifferentFilesTest()
        {
            File.WriteAllText("file1.txt", "Content1");
            File.WriteAllText("file2.txt", "Content2");

            var meta1 = new FileMeta("file1.txt", useHash: true);
            var meta2 = new FileMeta("file2.txt", useHash: true);

            Assert.False(meta1.Equals(meta2));

            File.Delete("file1.txt");
            File.Delete("file2.txt");
        }

        /// <summary>
        /// Checks that GetHashCode returns consistent value based on file metadata.
        /// </summary>
        [Fact]
        public void GetHashCodeTest()
        {
            File.WriteAllText(TempFile, "Hello World");

            var meta = new FileMeta(TempFile, useHash: true);

            int hash1 = meta.GetHashCode();
            int hash2 = meta.GetHashCode();

            Assert.Equal(hash1, hash2);

            File.Delete(TempFile);
        }
    }
}
