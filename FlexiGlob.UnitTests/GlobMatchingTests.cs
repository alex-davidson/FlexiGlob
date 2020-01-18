using System.Linq;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class GlobMatchingTests
    {
        [Test]
        public void FixedSegmentMatches()
        {
            var glob = new GlobParser().Parse("directory");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.False);
        }

        [Test]
        public void FixedSegmentMatchesOnlyOnce()
        {
            var glob = new GlobParser().Parse("directory");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory", "directory");

            Assert.That(child.IsMatch, Is.False);
            Assert.That(child.CanContinue, Is.False);
        }

        [Test]
        public void FixedSegmentYieldsPrefix()
        {
            var glob = new GlobParser().Parse("directory");
            var start = new GlobMatchFactory(true).Start(glob);

            var prefix = start.GetPrefixFilter();

            Assert.That(prefix, Is.EqualTo("directory"));
        }

        [Test]
        public void WildcardSegmentMatches()
        {
            var glob = new GlobParser().Parse("dir*ory");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.False);
        }

        [Test]
        public void WildcardSegmentMatchesOnlyOnce()
        {
            var glob = new GlobParser().Parse("dir*ory");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory", "directory");

            Assert.That(child.IsMatch, Is.False);
            Assert.That(child.CanContinue, Is.False);
        }

        [Test]
        public void WildcardSegmentYieldsPrefix()
        {
            var glob = new GlobParser().Parse("dir*ory");
            var start = new GlobMatchFactory(true).Start(glob);

            var prefix = start.GetPrefixFilter();

            Assert.That(prefix, Is.EqualTo("dir"));
        }

        [Test]
        public void WildcardSegmentMatchYieldsVariables()
        {
            var parser = new GlobParser
            {
                Variables =
                {
                    new GlobVariable("yyyy", @"\d{4}")
                }
            };
            var glob = parser.Parse("directory{yyyy}");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory2020");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.False);
            Assert.That(child.GetVariables().ToArray(), Is.EqualTo(new [] { new MatchedVariable("yyyy", "2020") }));
        }

        [Test]
        public void WildcardMultiSegmentMatches()
        {
            var glob = new GlobParser().Parse("**");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "directory");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.True);
        }

        [Test]
        public void WildcardMultiSegmentMatchesAnyDepth()
        {
            var glob = new GlobParser().Parse("**");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "a", "b", "c");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.True);
        }

        [Test]
        public void WildcardMultiSegmentDoesNotYieldPrefix()
        {
            var glob = new GlobParser().Parse("**");
            var start = new GlobMatchFactory(true).Start(glob);

            var prefix = start.GetPrefixFilter();

            Assert.That(prefix, Is.EqualTo(""));
        }

        [Test]
        public void WildcardMultiSegmentCanMatchNoSegments()
        {
            var glob = new GlobParser().Parse("a/**/b");
            var start = new GlobMatchFactory(true).Start(glob);

            var child = ApplyToHierarchy(start, "a", "b");

            Assert.That(child.IsMatch, Is.True);
            Assert.That(child.CanContinue, Is.True);
        }

        [Test]
        public void WildcardMultiSegmentCanMatchMultipleDepths()
        {
            var glob = new GlobParser().Parse("a/**/b/**/c");
            var start = new GlobMatchFactory(true).Start(glob);

            Assert.That(ApplyToHierarchy(start, "a", "b", "c").IsMatch, Is.True);
            Assert.That(ApplyToHierarchy(start, "a", "b", "c", "d").IsMatch, Is.False);
            Assert.That(ApplyToHierarchy(start, "a", "b", "c", "d", "c").IsMatch, Is.True);
            Assert.That(ApplyToHierarchy(start, "a", "c").IsMatch, Is.False);
            Assert.That(ApplyToHierarchy(start, "a", "d", "b", "c").IsMatch, Is.True);
        }

        private GlobMatch ApplyToHierarchy(GlobMatch child, params string[] pathSegments) => pathSegments.Aggregate(child, (m, p) => m.MatchChild(p));
    }
}
