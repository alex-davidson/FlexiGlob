using System;
using System.Diagnostics;
using System.IO;

namespace FlexiGlob.Support
{
    public class TemporaryDirectory : ITemporaryDirectory
    {
        /// <summary>
        /// Create a temporary directory in the default temp path.
        /// </summary>
        public TemporaryDirectory() : this(Path.GetTempPath())
        {
        }

        /// <summary>
        /// Create a temporary directory under the specified path.
        /// </summary>
        public TemporaryDirectory(string containingPath)
        {
            if (!Path.IsPathRooted(containingPath)) throw new ArgumentException($"Path is not absolute: {containingPath}", nameof(containingPath));
            var fullPath = Path.Combine(Path.GetFullPath(containingPath), Path.GetRandomFileName());
            if (File.Exists(fullPath)) throw new InvalidOperationException($"Cannot create temporary directory. Path already exists: {fullPath}");
            if (Directory.Exists(fullPath)) throw new InvalidOperationException($"Cannot create temporary directory. Path already exists: {fullPath}");

            FullPath = fullPath;
            Directory.CreateDirectory(fullPath);
        }

        public string FullPath { get; private set; }

        public void CleanUp()
        {
            if (Directory.Exists(FullPath))
            {
                Directory.Delete(FullPath, true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    CleanUp();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to clean up temporary directory: {FullPath}\n{ex}");
                }
                finally
                {
                    FullPath = null!;
                }
            }
            else
            {
                // Unsure if it's safe to do this in a finaliser...
                Debug.WriteLine("TemporaryDirectory was not disposed.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TemporaryDirectory()
        {
            Dispose(false);
        }
    }
}
