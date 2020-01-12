using System;
using System.Collections.Generic;
using System.Text;
using FlexiGlob.Comparers;
using NUnit.Framework;

namespace FlexiGlob.UnitTests.Comparers
{
    /// <summary>
    /// Compare two glob root segments for equality.
    /// </summary>
    [TestFixture]
    public class RootSegmentEqualityComparerTests
    {
        private readonly IEqualityComparer<RootSegment?> comparer = new RootSegmentEqualityComparer();

        [TestCase("//machine/share", "//machine/share")]
        [TestCase("//machine/share", "//MACHINE/share")]
        [TestCase("//machine/share", "//MACHINE/share/")]
        public void Equal(string a, string b)
        {
            var parser = new GlobParser();
            var aRoot = parser.Parse(a).Root;
            var bRoot = parser.Parse(b).Root;

            Assume.That(aRoot, Is.Not.Null);
            Assume.That(bRoot, Is.Not.Null);

            Assert.That(aRoot, Is.EqualTo(bRoot).Using(comparer));
            Assert.That(comparer.GetHashCode(aRoot!), Is.EqualTo(comparer.GetHashCode(bRoot!)));
        }

        [TestCase("//machine1/share", "//machine2/share")]
        [TestCase("//machine/share1", "//machine/share2")]
        public void NotEqual(string a, string b)
        {
            var parser = new GlobParser();
            var aRoot = parser.Parse(a).Root;
            var bRoot = parser.Parse(b).Root;

            Assume.That(aRoot, Is.Not.Null);
            Assume.That(bRoot, Is.Not.Null);

            Assert.That(aRoot, Is.Not.EqualTo(bRoot).Using(comparer));
        }

        [Test]
        public void Nulls()
        {
            Assert.That(comparer.Equals(null, null), Is.True);
            Assert.That(comparer.Equals(null, new LocalRootSegment("c:")), Is.False);
            Assert.That(comparer.Equals(new LocalRootSegment("c:"), null), Is.False);
        }
    }
}
