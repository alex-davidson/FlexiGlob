using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class WildcardSegmentTests
    {
        [Test]
        public void AcceptsMatchingCaseSensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<WildcardSegment>("dir*ory");

            var match = segment.Match("directory", true);

            Assert.That(match.Success, Is.True);
        }

        [Test]
        public void AcceptsMatchingCaseInsensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<WildcardSegment>("dir*ory");

            var match = segment.Match("DIRectoRy", false);

            Assert.That(match.Success, Is.True);
        }

        [Test]
        public void RejectsNonMatchingCaseSensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<WildcardSegment>("dir*ory");

            var match = segment.Match("DIRectoRy", true);

            Assert.That(match.Success, Is.False);
        }

        [Test]
        public void RejectsNonMatchingCaseInsensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<WildcardSegment>("dir*ory");

            var match = segment.Match("something", false);

            Assert.That(match.Success, Is.False);
        }

        [Test]
        public void ReturnsMatchedNamedGroupValues()
        {
            var parser = new GlobParser
            {
                Variables =
                {
                    new GlobVariable("yyyy", @"\d{4}"),
                    new GlobVariable("MM", @"\d{2}"),
                    new GlobVariable("dd", @"\d{2}"),
                }
            };
            var segment = parser.ParseSingleSegment<WildcardSegment>("{yyyy}-{MM}-{dd}");

            var match = segment.Match("2020-01-10", false);

            Assert.That(match.Success, Is.True);
            Assert.That(match.Variables,
                Is.EqualTo(new [] {
                    new MatchedVariable("yyyy", "2020"),
                    new MatchedVariable("MM", "01"),
                    new MatchedVariable("dd", "10"),
                }));
        }
    }
}
