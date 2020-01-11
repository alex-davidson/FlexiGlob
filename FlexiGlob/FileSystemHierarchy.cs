using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FlexiGlob
{
    public class FileSystemHierarchy : IGlobMatchableHierarchy<FileSystemInfo>
    {
        public FileSystemHierarchy(FileSystemInfo root, bool caseSensitive)
        {
            Root = root;
            CaseSensitive = caseSensitive;
        }

        public bool CaseSensitive { get; }
        public FileSystemInfo Root { get; }

        public string GetName(FileSystemInfo item) => item.Name;
        public bool IsContainer(FileSystemInfo item) => item is DirectoryInfo;

        public IEnumerable<FileSystemInfo> GetChildrenMatchingPrefix(FileSystemInfo item, string prefix) => item.GetChildrenMatchingPrefix(prefix, CaseSensitive);
    }
}
