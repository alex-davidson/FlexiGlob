using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class FixedSegmentTests
    {
        [Test]
        public void AcceptsMatchingCaseSensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<FixedSegment>("directory");

            var match = segment.Match("directory", true);

            Assert.That(match.Success, Is.True);
        }

        [Test]
        public void AcceptsMatchingCaseInsensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<FixedSegment>("directory");

            var match = segment.Match("DIRectoRy", false);

            Assert.That(match.Success, Is.True);
        }

        [Test]
        public void RejectsNonMatchingCaseSensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<FixedSegment>("directory");

            var match = segment.Match("DIRectoRy", true);

            Assert.That(match.Success, Is.False);
        }

        [Test]
        public void RejectsNonMatchingCaseInsensitivePathSegment()
        {
            var segment = new GlobParser().ParseSingleSegment<FixedSegment>("directory");

            var match = segment.Match("something", false);

            Assert.That(match.Success, Is.False);
        }
    }
}
