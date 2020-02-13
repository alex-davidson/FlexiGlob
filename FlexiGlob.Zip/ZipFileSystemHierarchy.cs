using System;
using System.Collections.Generic;
using System.IO;

namespace FlexiGlob.Zip
{
    public class ZipFileSystemHierarchy : IGlobMatchableHierarchy<FileSystemInfo>
    {
        private readonly ZipFileCache zipFileCache;

        public ZipFileSystemHierarchy(FileSystemInfo root, ITemporaryDirectory temporaryDirectory, bool suppressZipExtensions, bool caseSensitive)
        {
            this.zipFileCache = new ZipFileCache(temporaryDirectory, caseSensitive);
            this.SuppressZipExtensions = suppressZipExtensions;
            Root = root;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        /// Whether to silently suppress the extension of a ZIP file.
        /// </summary>
        /// <remarks>
        /// Allows ZIP archives to emulate directories. Note that this can lead to duplicate
        /// relative paths existing in the hierarchy.
        /// </remarks>
        public bool SuppressZipExtensions { get; }
        public bool CaseSensitive { get; }
        public FileSystemInfo Root { get; }

        public string GetName(FileSystemInfo item) => FilterName(item);
        public bool IsContainer(FileSystemInfo item) => item is DirectoryInfo || IsZipFile(item, out _);

        public IEnumerable<FileSystemInfo> GetChildrenMatchingPrefix(FileSystemInfo item, string prefix)
        {
            if (!IsZipFile(item, out var zipFile))
            {
                return new FileSystemHierarchy(item, CaseSensitive).GetChildrenMatchingPrefix(item, prefix);
            }

            var extracted = zipFileCache.ExtractToCache(zipFile);
            return new FileSystemHierarchy(extracted, CaseSensitive).GetChildrenMatchingPrefix(extracted, prefix);
        }

        public bool IsZipFile(FileSystemInfo item) => IsZipFile(item, out _);

        private string FilterName(FileSystemInfo item)
        {
            if (!SuppressZipExtensions) return item.Name;
            if (!IsZipFile(item, out _)) return item.Name;
            return Path.GetFileNameWithoutExtension(item.Name);
        }

        private static bool IsZipFile(FileSystemInfo item, out FileInfo zipFile)
        {
            if (item is FileInfo fileInfo)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(fileInfo.Extension, ".zip"))
                {
                    zipFile = fileInfo;
                    return true;
                }
            }
            zipFile = null!;
            return false;
        }
    }
}
