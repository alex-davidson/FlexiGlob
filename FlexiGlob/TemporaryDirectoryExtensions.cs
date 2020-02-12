using System.IO;

namespace FlexiGlob
{
    public static class TemporaryDirectoryExtensions
    {
        /// <summary>
        /// Combines the temporary directory's full path with a list of relative paths or path segments, and returns the absolute path.
        /// </summary>
        public static string GetPath(this ITemporaryDirectory temporaryDirectory, params string[] pathSegments) =>
            Path.Combine(temporaryDirectory.FullPath, Path.Combine(pathSegments));
    }
}
