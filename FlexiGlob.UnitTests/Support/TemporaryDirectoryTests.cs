﻿using System.IO;
using FlexiGlob.Support;
using NUnit.Framework;

namespace FlexiGlob.UnitTests.Support
{
    [TestFixture]
    public class TemporaryDirectoryTests
    {
        [Test]
        public void CleanUpRemovesDirectory()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                Assume.That(temporaryDirectory.FullPath, Does.Exist);

                temporaryDirectory.CleanUp();

                Assert.That(temporaryDirectory.FullPath, Does.Not.Exist);
            }
        }

        [Test]
        public void DisposeRemovesDirectory()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var fullPath = temporaryDirectory.FullPath;
                Assume.That(fullPath, Does.Exist);

                temporaryDirectory.Dispose();

                Assert.That(fullPath, Does.Not.Exist);
            }
        }

        [Test]
        public void CleanUpRemovesNotEmptyDirectory()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                Assume.That(temporaryDirectory.FullPath, Does.Exist);
                File.WriteAllText(Path.Combine(temporaryDirectory.FullPath, "test.txt"), "");

                temporaryDirectory.CleanUp();

                Assert.That(temporaryDirectory.FullPath, Does.Not.Exist);
            }
        }

        [Test]
        public void DisposeRemovesNotEmptyDirectory()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var fullPath = temporaryDirectory.FullPath;
                Assume.That(fullPath, Does.Exist);
                File.WriteAllText(Path.Combine(temporaryDirectory.FullPath, "test.txt"), "");

                temporaryDirectory.Dispose();

                Assert.That(fullPath, Does.Not.Exist);
            }
        }
    }
}
