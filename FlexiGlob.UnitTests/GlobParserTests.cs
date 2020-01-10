using System.Collections.Generic;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class GlobParserTests
    {
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

            Assert.That(glob.Root, Is.EqualTo(testCase.Root).Using(new RootSegmentEqualityComparer()));
            Assert.That(glob.Segments, Is.EqualTo(testCase.Segments).Using(new SegmentEqualityComparer()));
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

            Assert.That(glob.Root, Is.Null);
            Assert.That(glob.Segments,
                Is.EqualTo(new []
                {
                    new WildcardSegment("logs-{yyyy}", @"^logs-(?<yyyy>(\d{4}))$", "logs-"),
                    new WildcardSegment("{MM}{dd}", @"^(?<MM>(\d{2}))(?<dd>(\d{2}))$", ""),
                    new WildcardSegment("*.log", @"^.*\.log$", ""),
                })
                .Using(new SegmentEqualityComparer()));
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

            Assert.That(glob.Segments,
                Is.EqualTo(new Segment[]
                {
                    new FixedSegment("a"),
                    new WildcardMultiSegment(),
                    new FixedSegment("b"),
                    new WildcardMultiSegment(),
                    new FixedSegment("c"),
                })
                .Using(new SegmentEqualityComparer()));
        }

        public class Case
        {
            public string? Pattern { get; set; }
            public RootSegment? Root { get; set; }
            public List<Segment> Segments { get; } = new List<Segment>();

            public override string ToString() => Pattern ?? "";
        }

        private class RootSegmentEqualityComparer : IEqualityComparer<RootSegment>
        {
            public bool Equals(RootSegment? x, RootSegment? y)
            {
                if (ReferenceEquals(x, y)) return true;
                switch (x, y)
                {
                    case (UNCRootSegment a, UNCRootSegment b): return a.Machine == b.Machine && a.Share == b.Share;
                    case (LocalRootSegment a, LocalRootSegment b): return a.Token == b.Token;
                    default: return false;
                }
            }

            public int GetHashCode(RootSegment obj)
            {
                return obj.Token.GetHashCode();
            }
        }

        private class SegmentEqualityComparer : IEqualityComparer<Segment>
        {
            public bool Equals(Segment? x, Segment? y)
            {
                if (ReferenceEquals(x, y)) return true;
                switch (x, y)
                {
                    case (FixedSegment a, FixedSegment b): return a.Token == b.Token;
                    case (WildcardMultiSegment a, WildcardMultiSegment b): return true;
                    case (WildcardSegment a, WildcardSegment b): return a.Regex == b.Regex && a.Prefix == b.Prefix;
                    default: return false;
                }
            }

            public int GetHashCode(Segment obj)
            {
                return obj.Token.GetHashCode();
            }
        }
    }
}
