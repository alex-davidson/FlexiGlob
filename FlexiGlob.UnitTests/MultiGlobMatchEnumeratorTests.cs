using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class MultiGlobMatchEnumeratorTests
    {
        [Test]
        public void FindsAllMatchesOfSingleInclude()
        {
            var hierarchy = new TestHierarchy(
                new SimpleMatchable("root",
                    new SimpleMatchable("dev",
                        new SimpleMatchable("hda"),
                        new SimpleMatchable("hda1"),
                        new SimpleMatchable("hdb"),
                        new SimpleMatchable("hdb1"),
                        new SimpleMatchable("hdb2")),
                    new SimpleMatchable("home",
                        new SimpleMatchable("me",
                            new SimpleMatchable("hdmi")),
                        new SimpleMatchable("you")),
                    new SimpleMatchable("var")));

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/hd??")
            };

            var matches = new MultiGlobMatchEnumerator(includes, Glob.None).EnumerateMatches(hierarchy);

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda1",
                    "hdb1",
                    "hdb2",
                    "hdmi"
                }));
        }

        [Test]
        public void FindsAllUniqueMatchesOfMultipleIncludes()
        {
            var hierarchy = new TestHierarchy(
                new SimpleMatchable("root",
                    new SimpleMatchable("dev",
                        new SimpleMatchable("hda"),
                        new SimpleMatchable("hda1"),
                        new SimpleMatchable("hdb"),
                        new SimpleMatchable("hdb1"),
                        new SimpleMatchable("hdb2")),
                    new SimpleMatchable("home",
                        new SimpleMatchable("me",
                            new SimpleMatchable("hdmi")),
                        new SimpleMatchable("you")),
                    new SimpleMatchable("var")));

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/hd[am]?"),
                parser.Parse("home/**")
            };

            var matches = new MultiGlobMatchEnumerator(includes, Glob.None).EnumerateMatches(hierarchy);

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda1",
                    "me",
                    "hdmi",
                    "you"
                }));
        }

        [Test]
        public void FindsAllUniqueMatchesOfIncludesNotMatchedByExcludes()
        {
            var hierarchy = new TestHierarchy(
                new SimpleMatchable("root",
                    new SimpleMatchable("dev",
                        new SimpleMatchable("hda"),
                        new SimpleMatchable("hda1"),
                        new SimpleMatchable("hdb"),
                        new SimpleMatchable("hdb1"),
                        new SimpleMatchable("hdb2")),
                    new SimpleMatchable("home",
                        new SimpleMatchable("me",
                            new SimpleMatchable("hdmi")),
                        new SimpleMatchable("you")),
                    new SimpleMatchable("var")));

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/h*")
            };
            var excludes = new []
            {
                parser.Parse("home/*/**")
            };

            var matches = new MultiGlobMatchEnumerator(includes, excludes).EnumerateMatches(hierarchy);

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda",
                    "hda1",
                    "hdb",
                    "hdb1",
                    "hdb2",
                    "home"
                }));
        }

        [Test]
        public void ContainerExclusionDoesNotApplyRecursively()
        {
            var hierarchy = new TestHierarchy(
                new SimpleMatchable("root",
                    new SimpleMatchable("dev",
                        new SimpleMatchable("hda"),
                        new SimpleMatchable("hda1"),
                        new SimpleMatchable("hdb"),
                        new SimpleMatchable("hdb1"),
                        new SimpleMatchable("hdb2")),
                    new SimpleMatchable("home",
                        new SimpleMatchable("me",
                            new SimpleMatchable("hdmi")),
                        new SimpleMatchable("you")),
                    new SimpleMatchable("var")));

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/h*")
            };
            var excludes = new []
            {
                parser.Parse("home")
            };

            var matches = new MultiGlobMatchEnumerator(includes, excludes).EnumerateMatches(hierarchy);

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda",
                    "hda1",
                    "hdb",
                    "hdb1",
                    "hdb2",
                    "hdmi"
                }));
        }

        [Test]
        public void YieldsMatchDetailsPerIncludeGlob()
        {
            var hierarchy = new TestHierarchy(
                new SimpleMatchable("root",
                    new SimpleMatchable("dev",
                        new SimpleMatchable("hda"),
                        new SimpleMatchable("hda1"),
                        new SimpleMatchable("hdb"),
                        new SimpleMatchable("hdb1"),
                        new SimpleMatchable("hdb2")),
                    new SimpleMatchable("home",
                        new SimpleMatchable("me",
                            new SimpleMatchable("hdmi")),
                        new SimpleMatchable("you")),
                    new SimpleMatchable("var")));

            var parser = new GlobParser();
            var includes = new []
            {
                parser.Parse("**/hd[am]?"),
                parser.Parse("home/**")
            };

            var matches = new MultiGlobMatchEnumerator(includes, Glob.None).EnumerateMatches(hierarchy).ToArray();

            Assume.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda1",
                    "me",
                    "hdmi",
                    "you"
                }));

            var hdmiMatch = matches.Single(m => m.Item.Name == "hdmi");

            Assert.That(hdmiMatch.Details.Select(s => s.Glob).ToArray(), Is.EquivalentTo(includes));
        }

        private struct SimpleMatchable
        {
            public string Name { get; }
            public SimpleMatchable[] Children { get; }

            public SimpleMatchable(string name, params SimpleMatchable[] children)
            {
                Name = name;
                Children = children;
            }
        }

        private class TestHierarchy : IGlobMatchableHierarchy<SimpleMatchable>
        {
            public TestHierarchy(SimpleMatchable root)
            {
                Root = root;
            }

            public bool CaseSensitive => true;
            public SimpleMatchable Root { get; }
            public string GetName(SimpleMatchable item) => item.Name;
            public bool IsContainer(SimpleMatchable item) => item.Children.Any();
            public IEnumerable<SimpleMatchable> GetChildrenMatchingPrefix(SimpleMatchable item, string prefix)
            {
                return item.Children.Where(c => c.Name.StartsWith(prefix));
            }
        }
    }
}
