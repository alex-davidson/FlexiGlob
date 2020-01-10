using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class WildcardMultiSegmentTests
    {
        [TestCase("a load of stuff", true)]
        [TestCase("a load of stuff", false)]
        [TestCase("junk/paths", true)]
        [TestCase("a/B/c", false)]
        public void AcceptsAnything(string value, bool caseSensitive)
        {
            var segment = new GlobParser().ParseSingleSegment<WildcardMultiSegment>("**");

            var match = segment.Match(value, caseSensitive);

            Assert.That(match.Success, Is.True);
        }
    }
}
