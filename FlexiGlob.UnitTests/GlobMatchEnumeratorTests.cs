using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class GlobMatchEnumeratorTests
    {
        [Test]
        public void FindsAllMatches()
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

            var glob = new GlobParser().Parse("**/hd??");

            var matches = new GlobMatchEnumerator(glob).EnumerateMatches(hierarchy);

            Assert.That(matches.Select(m => m.Item.Name).ToArray(),
                Is.EquivalentTo(new []
                {
                    "hda1",
                    "hdb1",
                    "hdb2",
                    "hdmi"
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
