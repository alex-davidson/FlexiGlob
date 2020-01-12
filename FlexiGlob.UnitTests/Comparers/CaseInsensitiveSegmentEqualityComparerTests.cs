using System.Collections.Generic;
using FlexiGlob.Comparers;
using NUnit.Framework;

namespace FlexiGlob.UnitTests.Comparers
{
    [TestFixture]
    public class CaseInsensitiveSegmentEqualityComparerTests
    {
        private readonly IEqualityComparer<Segment?> comparer = SegmentEqualityComparer.CaseInsensitive;
        private readonly GlobParser parser = new GlobParser
        {
            Variables =
            {
                new GlobVariable("yyyy", "."),
                new GlobVariable("YYYY", "."),
                new GlobVariable("mm", ".")
            }
        };

        [TestCase("directory", "directory")]
        [TestCase("directory", "Directory")]
        [TestCase("**", "**")]
        [TestCase("Prog*", "Prog*")]
        [TestCase("Prog*", "prog*")]
        [TestCase("temp{mm}", "Temp{mm}")]
        [TestCase("?{yyyy}*", "?{yyyy}*")]
        [TestCase("test[a-c]", "test[a-c]")]
        [TestCase("test[a-c]", "test[A-c]")]
        public void Equal(string a, string b)
        {
            var aSegment = parser.ParseSingleSegment<Segment>(a);
            var bSegment = parser.ParseSingleSegment<Segment>(b);

            Assume.That(aSegment, Is.Not.Null);
            Assume.That(bSegment, Is.Not.Null);

            Assert.That(aSegment, Is.EqualTo(bSegment).Using(comparer));
            Assert.That(comparer.GetHashCode(aSegment!), Is.EqualTo(comparer.GetHashCode(bSegment!)));
        }

        [TestCase("prog*", "prog*m")]
        [TestCase("?{yyyy}*", "?{YYYY}*")]
        public void NotEqual(string a, string b)
        {
            var aSegment = parser.ParseSingleSegment<Segment>(a);
            var bSegment = parser.ParseSingleSegment<Segment>(b);

            Assume.That(aSegment, Is.Not.Null);
            Assume.That(bSegment, Is.Not.Null);

            Assert.That(aSegment, Is.Not.EqualTo(bSegment).Using(comparer));
        }

        [Test]
        public void Nulls()
        {
            Assert.That(comparer.Equals(null, null), Is.True);
            Assert.That(comparer.Equals(null, new FixedSegment("dir")), Is.False);
            Assert.That(comparer.Equals(new FixedSegment("dir"), null), Is.False);
        }
    }
}
