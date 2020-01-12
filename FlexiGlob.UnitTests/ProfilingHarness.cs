using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class ProfilingHarness
    {
        [Test, Explicit]
        public void ExerciseMultiGlobMatchEnumerator()
        {
            TestContext.WriteLine($"CLR Version: {Environment.Version}");
            TestContext.WriteLine($"Runtime: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/Microsoft*/**/*.dll"),
                parser.Parse("Program*/**/*.exe"),
            };
            var excludes = new []
            {
                parser.Parse("**/SQL*/**")
            };

            var hierarchy = new FileSystemHierarchy(new DirectoryInfo("C:\\"), false);
            var enumerator = new MultiGlobMatchEnumerator()
                .Exclude(excludes)
                .Include(includes);

            var count = enumerator.EnumerateMatches(hierarchy).Count();

            TestContext.WriteLine($"{count} matches");
        }
    }
}
