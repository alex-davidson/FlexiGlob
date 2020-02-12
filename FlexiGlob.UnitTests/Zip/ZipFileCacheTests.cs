using System.IO;
using System.IO.Compression;
using FlexiGlob.Support;
using FlexiGlob.Zip;
using NUnit.Framework;

namespace FlexiGlob.UnitTests.Zip
{
    [TestFixture]
    public class ZipFileCacheTests
    {
        [Test]
        public void ReusesExtractedCopy()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    zip.CreateEntry("fileA");
                    zip.CreateEntry("fileB");
                    zip.CreateEntry("fileC");
                }

                var zipFile = new FileInfo(temporaryDirectory.GetPath("test.zip"));

                using (var buffer = new TemporaryDirectory())
                {
                    var cache = new ZipFileCache(buffer, caseSensitive: false);

                    var extracted1 = cache.ExtractToCache(zipFile);
                    var extracted2 = cache.ExtractToCache(zipFile);

                    Assert.That(extracted1, Is.EqualTo(extracted2));
                }
            }
        }

        [Test]
        public void CreatesNewExtractedCopyForDifferentZipFile()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                Directory.CreateDirectory(temporaryDirectory.GetPath("A"));
                using (var zipA = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("A", "test.zip")), ZipArchiveMode.Create))
                {
                    zipA.CreateEntry("fileA");
                    zipA.CreateEntry("fileB");
                    zipA.CreateEntry("fileC");
                }
                Directory.CreateDirectory(temporaryDirectory.GetPath("B"));
                using (var zipB = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("B", "test.zip")), ZipArchiveMode.Create))
                {
                    zipB.CreateEntry("fileA");
                    zipB.CreateEntry("fileB");
                    zipB.CreateEntry("fileC");
                }

                var zipFileA = new FileInfo(temporaryDirectory.GetPath("A", "test.zip"));
                var zipFileB = new FileInfo(temporaryDirectory.GetPath("B", "test.zip"));

                using (var buffer = new TemporaryDirectory())
                {
                    var cache = new ZipFileCache(buffer, caseSensitive: false);

                    var extractedA = cache.ExtractToCache(zipFileA);
                    var extractedB = cache.ExtractToCache(zipFileB);

                    Assert.That(extractedA, Is.Not.EqualTo(extractedB));
                }
            }
        }
    }
}
