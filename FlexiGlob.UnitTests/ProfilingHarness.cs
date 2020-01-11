using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class ProfilingHarness
    {
        [Test, Explicit]
        public void ExerciseMultiGlobMatchEnumerator()
        {
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

            var hierarchy = new FileSystemHierarchy(new DirectoryInfo("C:"), false);
            var enumerator = new MultiGlobMatchEnumerator(includes, excludes);

            var count = enumerator.EnumerateMatches(hierarchy).Count();
        }
    }
}
