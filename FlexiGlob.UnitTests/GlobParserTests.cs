using System.Collections.Generic;
using FlexiGlob.Comparers;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class GlobParserTests
    {
        private static readonly IEqualityComparer<Segment> segmentComparer = SegmentEqualityComparer.CaseSensitive;
        private static readonly IEqualityComparer<RootSegment> rootSegmentComparer = new RootSegmentEqualityComparer();
        private static readonly IEqualityComparer<Glob> globComparer = GlobEqualityComparer.CaseSensitive;

        public static Case[] Valid =
        {
            new Case
            {
                Pattern = ""
            },
            new Case
            {
                Pattern = "**",
                Segments = { new WildcardMultiSegment() },
            },
            new Case
            {
                Pattern = @"C:/temp/test.png",
                Root = new LocalRootSegment("C:"),
                Segments = { new FixedSegment("temp"), new FixedSegment("test.png") },
            },
            new Case
            {
                Pattern = @"/temp/test.png",
                Root = new LocalRootSegment(),
                Segments = { new FixedSegment("temp"), new FixedSegment("test.png") },
            },
            new Case
            {
                Pattern = @"temp/test.png",
                Segments = { new FixedSegment("temp"), new FixedSegment("test.png") },
            },
            new Case
            {
                Pattern = @"c:/temp/**/test.png",
                Root = new LocalRootSegment("c:"),
                Segments = { new FixedSegment("temp"), new WildcardMultiSegment(), new FixedSegment("test.png") },
            },
            new Case
            {
                Pattern = @"c:/temp/**/dir*/",
                Root = new LocalRootSegment("c:"),
                Segments = { new FixedSegment("temp"), new WildcardMultiSegment(), new WildcardSegment("dir*", "^dir.*$", "dir") },
            },
            new Case
            {
                Pattern = @"C:/temp/**/dir*/archive.zip/**",
                Root = new LocalRootSegment("C:"),
                Segments = { new FixedSegment("temp"), new WildcardMultiSegment(), new WildcardSegment("dir*", "^dir.*$", "dir"), new FixedSegment("archive.zip"), new WildcardMultiSegment() },
            },
            new Case
            {
                Pattern = @"/temp/**/dir*/archive.zip/**",
                Root = new LocalRootSegment(),
                Segments = { new FixedSegment("temp"), new WildcardMultiSegment(), new WildcardSegment("dir*", "^dir.*$", "dir"), new FixedSegment("archive.zip"), new WildcardMultiSegment() },
            },
            new Case
            {
                Pattern = @"/temp/**/dir*/archive.zip/**",
                Root = new LocalRootSegment(),
                Segments = { new FixedSegment("temp"), new WildcardMultiSegment(), new WildcardSegment("dir*", "^dir.*$", "dir"), new FixedSegment("archive.zip"), new WildcardMultiSegment() },
            },
            new Case
            {
                Pattern = @"//machine/publicshare/**/*.zip",
                Root = new UNCRootSegment(@"\\machine\publicshare", "machine", "publicshare"),
                Segments = { new WildcardMultiSegment(), new WildcardSegment("*.zip", @"^.*\.zip$") },
            },
            new Case
            {
                Pattern = @"C:/temp/test?.png",
                Root = new LocalRootSegment("C:"),
                Segments = { new FixedSegment("temp"), new WildcardSegment("test?.png", @"^test.\.png$", "test") },
            },
            new Case
            {
                Pattern = @"C:/temp/test[a-h].png",
                Root = new LocalRootSegment("C:"),
                Segments = { new FixedSegment("temp"), new WildcardSegment("test[a-h].png", @"^test[a-h]\.png$", "test") },
            },
            new Case
            {
                Pattern = @"c:/temp/*t[es]t*.png",
                Root = new LocalRootSegment("c:"),
                Segments = { new FixedSegment("temp"), new WildcardSegment("*t[es]t*.png", @"^.*t[es]t.*\.png$") },
            },
            new Case
            {
                Pattern = @"/c:/temp",
                Root = new LocalRootSegment(),
                Segments = { new FixedSegment("c:"), new FixedSegment("temp") },
            },
            new Case
            {
                Pattern = @"./c:/temp",
                Root = null,
                Segments = { new FixedSegment("c:"), new FixedSegment("temp") },
            },
            new Case
            {
                Pattern = @"./c:/temp\/\ stuff/subdir",
                Root = null,
                Segments = { new FixedSegment("c:"), new FixedSegment("temp/ stuff"), new FixedSegment("subdir") },
            },
        };

        public static string[] Invalid =
        {
            @"//c:/temp",           // UNC-like path containing a drive specifier for a machine name.
            @"//machine/c:",        // UNC-like path containing a drive specifier for a share name.
            @"//machine/c:/",       // UNC-like path containing a drive specifier for a share name.
            @"///c:/",              // UNC-like path containing a drive specifier for a share name and no machine name.
            @"//machine",           // UNC-like path with no share name.
            @"//machine/",          // UNC-like path with no share name.
            @"//machine/ /",        // UNC-like path with whitespace share name.
            @"//machine/share//",   // Repeated path separator.
            @"///share/",           // UNC-like path with no machine name.
            @"c:/temp/../*",        // Upward relative path segment.
            @"c:/temp/./*",         // No-op relative path segment.
            @"c:/temp//*",          // Repeated path separator.
            @"c://",                // Repeated path separator.
            @"c://temp",            // Repeated path separator.
            @"c:/temp**",           // Multi-segment wildcard combined with prefix.
            @"c:/temp[a-h/]",       // Range wildcard extends beyond the segment.
            @"c:/temp[a-h/",        // Range wildcard is not closed.
            @".//c:/",              // Repeated path separator.
        };

        [TestCaseSource(nameof(Valid))]
        public void ParsesValidPatterns(Case testCase)
        {
            var glob = new GlobParser().Parse(testCase.Pattern!);

            Assert.That(glob.Root, Is.EqualTo(testCase.Root).Using(rootSegmentComparer));
            Assert.That(glob.Segments, Is.EqualTo(testCase.Segments).Using(segmentComparer));
            Assert.That(glob, Is.EqualTo(new Glob(testCase.Root, testCase.Segments.ToArray())).Using(globComparer));
        }

        [TestCaseSource(nameof(Invalid))]
        public void RejectsInvalidPatterns(string pattern)
        {
            Assert.That(() => new GlobParser().Parse(pattern), Throws.InstanceOf<GlobFormatException>());
        }

        [Test]
        public void IncludesValidVariableCaptures()
        {
            var parser = new GlobParser
            {
                Variables =
                {
                    new GlobVariable("yyyy", @"\d{4}"),
                    new GlobVariable("MM", @"\d{2}"),
                    new GlobVariable("dd", @"\d{2}")
                }
            };

            var glob = parser.Parse("./logs-{yyyy}/{MM}{dd}/*.log");

            var expected = new Glob(null,
                new WildcardSegment("logs-{yyyy}", @"^logs-(?<yyyy>(\d{4}))$", "logs-"),
                new WildcardSegment("{MM}{dd}", @"^(?<MM>(\d{2}))(?<dd>(\d{2}))$", ""),
                new WildcardSegment("*.log", @"^.*\.log$", ""));

            Assert.That(glob.Root, Is.EqualTo(expected.Root).Using(rootSegmentComparer));
            Assert.That(glob.Segments, Is.EqualTo(expected.Segments).Using(segmentComparer));
            Assert.That(glob, Is.EqualTo(expected).Using(globComparer));
        }

        [Test]
        public void RejectsUndefinedVariableCaptures()
        {
            var parser = new GlobParser
            {
                Variables =
                {
                    new GlobVariable("MM", @"\d{2}"),
                    new GlobVariable("dd", @"\d{2}")
                }
            };

            Assert.That(() => parser.Parse("./logs-{missing}/{MM}{dd}/*.log"), Throws.InstanceOf<GlobFormatException>());
        }

        [Test]
        public void CoalescesAdjacentWildcardMultiSegments()
        {
            var glob = new GlobParser().Parse("a/**/**/**/b/**/c");

            var expected = new Glob(null,
                new FixedSegment("a"),
                new WildcardMultiSegment(),
                new FixedSegment("b"),
                new WildcardMultiSegment(),
                new FixedSegment("c"));

            Assert.That(glob.Root, Is.EqualTo(expected.Root).Using(rootSegmentComparer));
            Assert.That(glob.Segments, Is.EqualTo(expected.Segments).Using(segmentComparer));
            Assert.That(glob, Is.EqualTo(expected).Using(globComparer));
        }

        public class Case
        {
            public string? Pattern { get; set; }
            public RootSegment? Root { get; set; }
            public List<Segment> Segments { get; } = new List<Segment>();

            public override string ToString() => Pattern ?? "";
        }
    }
}
