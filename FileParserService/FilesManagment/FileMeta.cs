using System.Security.Cryptography;

namespace FileParserService.FilesManagment
{
    /// <summary>
    /// Stores metadata about a file and provides methods for change detection.
    /// </summary>
    public class FileMeta
    {
        public DateTime LastWrite { get; set; }
        public long Size { get; set; }
        public string? Hash { get; set; }

        /// <param name="filePath">Path to the file.</param>
        /// <param name="useHash">Whether to compute the hash for content-based change detection.</param>
        public FileMeta(string filePath, bool useHash)
        {
            var info = new FileInfo(filePath);
            LastWrite = info.LastWriteTimeUtc;
            Size = info.Length;
            Hash = useHash ? ComputeHash(filePath) : null;
        }

        /// <summary>
        /// Determines whether this instance is equal to another.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if files are considered identical; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not FileMeta other)
                return false;

            if (LastWrite != other.LastWrite || Size != other.Size)
                return false;

            //compare hash if we useв flag useHash
            if (Hash != null && other.Hash != null)
                return Hash == other.Hash;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this file metadata instance.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(LastWrite, Size, Hash);

        /// <summary>
        /// Computes the SHA-256 hash of a file's content.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>Hexadecimal string representation of the file hash.</returns>
        public static string ComputeHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }
    }
}
