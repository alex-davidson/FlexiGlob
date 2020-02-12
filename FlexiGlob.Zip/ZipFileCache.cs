using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace FlexiGlob.Zip
{
    internal class ZipFileCache
    {
        private readonly ITemporaryDirectory temporaryDirectory;
        private readonly bool caseSensitive;

        public ZipFileCache(ITemporaryDirectory temporaryDirectory, bool caseSensitive)
        {
            this.temporaryDirectory = temporaryDirectory;
            this.caseSensitive = caseSensitive;
        }

        /// <summary>
        /// Extract the ZIP file into a temporary directory and return that directory.
        /// </summary>
        public FileSystemInfo ExtractToCache(FileInfo zipFile)
        {
            var tempName = GenerateTempName(zipFile);
            var zipDirectoryPath = temporaryDirectory.GetPath(tempName);

            // Already requested and extracted?
            if (!Directory.Exists(zipDirectoryPath))
            {
                // Create parent directory if necessary.
                Directory.CreateDirectory(temporaryDirectory.FullPath);

                ZipFile.ExtractToDirectory(zipFile.FullName, zipDirectoryPath);
            }

            return new DirectoryInfo(zipDirectoryPath);
        }

        private string GenerateTempName(FileSystemInfo item)
        {
            var name = caseSensitive ? item.FullName : item.FullName.ToLowerInvariant();
            using (var sha = SHA256.Create())
            {
                return Util.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(name)));
            }
        }
    }
}
