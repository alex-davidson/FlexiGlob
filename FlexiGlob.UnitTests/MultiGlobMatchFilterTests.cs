using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class MultiGlobMatchFilterTests
    {
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

            var enumerator = new MultiGlobMatchEnumerator().Include(parser.Parse("**"));
            var filter = new MultiGlobMatchFilter(hierarchy.CaseSensitive)
                .Exclude(excludes)
                .Include(includes);

            var matches = enumerator.EnumerateMatches(hierarchy)
                .Where(filter.Filter)
                .ToArray();

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
        public void EarlierRulesTakePrecedence_IncludeBeforeExclude()
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

            var enumerator = new MultiGlobMatchEnumerator().Include(parser.Parse("**"));
            var filter = new MultiGlobMatchFilter(hierarchy.CaseSensitive)
                .Include(parser.Parse("**/hd[am]?"))
                .Exclude(parser.Parse("dev/**"));

            var matches = enumerator.EnumerateMatches(hierarchy)
                .Where(filter.Filter)
                .ToArray();

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda1",
                    "hdmi",
                }));
        }

        [Test]
        public void EarlierRulesTakePrecedence_ExcludeBeforeInclude()
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

            var enumerator = new MultiGlobMatchEnumerator().Include(parser.Parse("**"));
            var filter = new MultiGlobMatchFilter(hierarchy.CaseSensitive)
                .Exclude(parser.Parse("dev/**"))
                .Include(parser.Parse("**/hd[am]?"));

            var matches = enumerator.EnumerateMatches(hierarchy)
                .Where(filter.Filter)
                .ToArray();

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hdmi",
                }));
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
