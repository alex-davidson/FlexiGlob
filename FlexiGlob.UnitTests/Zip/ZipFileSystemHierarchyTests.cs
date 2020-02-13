using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using FlexiGlob.Support;
using FlexiGlob.Zip;
using NUnit.Framework;

namespace FlexiGlob.UnitTests.Zip
{
    [TestFixture]
    public class ZipFileSystemHierarchyTests
    {
        [Test]
        public void CanEnumerateFilesWithinZipArchive()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    zip.CreateEntry("fileA");
                    zip.CreateEntry("fileB");
                    zip.CreateEntry("fileC");
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: false, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("test*/file*"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assert.That(results, Has.Length.EqualTo(3));
                    Assert.That(results.Select(r => r.Item.Exists), Has.All.True);
                }
            }
        }

        [Test]
        public void CanEnumerateFilesRecursivelyWithinZipArchives()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("nestedA");
                        nestedZip.CreateEntry("nestedB");
                    }
                    zip.CreateEntry("outerA");
                    zip.CreateEntry("outerB");
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: false, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("test*/**/*A"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assert.That(results, Has.Length.EqualTo(2));
                    Assert.That(results.Select(r => r.Item.Exists), Has.All.True);
                }
            }
        }

        [Test]
        public void SuppressedZipExtensionsAreNotMatched()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("nestedA");
                        nestedZip.CreateEntry("nestedB");
                    }
                    zip.CreateEntry("outerA");
                    zip.CreateEntry("outerB");
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: true, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("test*/*.zip/*A"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assert.That(results, Is.Empty);
                }
            }
        }

        [Test]
        public void SuppressedZipExtensionsAreNotPresentInRelativePath()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("inside/archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("directory/nestedFile.txt");
                    }
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: true, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("**/*.txt"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assume.That(results, Has.Length.EqualTo(1));
                    Assert.That(results.Single().Details.GetPathSegments(), Is.EqualTo(new [] { "test", "inside", "archive", "directory", "nestedFile.txt" }));
                }
            }
        }

        [Test]
        public void RelativePathReflectsZipHierarchy()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("inside/archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("directory/nestedFile.txt");
                    }
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: false, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("**/*.txt"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assume.That(results, Has.Length.EqualTo(1));
                    Assert.That(results.Single().Details.GetPathSegments(), Is.EqualTo(new [] { "test.zip", "inside", "archive.zip", "directory", "nestedFile.txt" }));
                }
            }
        }

        [Test]
        public void CanFilterOutZipfileMatchesInCaller()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("inside/archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("directory/nestedFile.txt");
                    }
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new DirectoryInfo(temporaryDirectory.FullPath), buffer, suppressZipExtensions: false, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("**"));

                    var results = matcher.EnumerateMatches(hierarchy).Where(m => m.Item is FileInfo && !hierarchy.IsZipFile(m.Item)).ToArray();

                    Assert.That(results, Has.Length.EqualTo(1));
                    Assert.That(results.Single().Details.GetPathSegments(), Is.EqualTo(new [] { "test.zip", "inside", "archive.zip", "directory", "nestedFile.txt" }));
                }
            }
        }

        [Test]
        public void CanUseZipFileAsHierarchyRoot()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                using (var zip = new ZipArchive(File.OpenWrite(temporaryDirectory.GetPath("test.zip")), ZipArchiveMode.Create))
                {
                    var nested = zip.CreateEntry("inside/archive.zip");
                    using (var nestedZip = new ZipArchive(nested.Open(), ZipArchiveMode.Create))
                    {
                        nestedZip.CreateEntry("directory/nestedFile.txt");
                    }
                }

                using (var buffer = new TemporaryDirectory())
                {
                    var hierarchy = new ZipFileSystemHierarchy(new FileInfo(temporaryDirectory.GetPath("test.zip")), buffer, suppressZipExtensions: false, caseSensitive: false);
                    var matcher = new GlobMatchEnumerator(new GlobParser().Parse("**/*.txt"));

                    var results = matcher.EnumerateMatches(hierarchy).ToArray();

                    Assume.That(results, Has.Length.EqualTo(1));
                    Assert.That(results.Single().Details.GetPathSegments(), Is.EqualTo(new [] { "inside", "archive.zip", "directory", "nestedFile.txt" }));
                }
            }
        }
    }
}
