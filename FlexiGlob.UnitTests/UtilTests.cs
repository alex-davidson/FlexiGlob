using System.Collections.Generic;
using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class UtilTests
    {
        [Test]
        public void LongestCommonPrefix_RejectsEmptyList()
        {
            Assert.That(() => Util.LongestCommonPrefix(new List<string>(), true), Throws.ArgumentException);
        }

        [Test]
        public void LongestCommonPrefix_CaseSensitive()
        {
            var prefix = Util.LongestCommonPrefix(new List<string>
            {
                "abcdefgh",
                "abcdEfgh",
                "abcdegh",
                "abcDefgh",
                "abcdefgh",
            }, true);

            Assert.That(prefix, Is.EqualTo("abc"));
        }

        [Test]
        public void LongestCommonPrefix_CaseInsensitive()
        {
            var prefix = Util.LongestCommonPrefix(new List<string>
            {
                "abcdefgh",
                "abcdEfgh",
                "abcdegh",
                "abcDefgh",
                "abcdefgh",
            }, false);

            Assert.That(prefix, Is.EqualTo("abcde"));
        }
    }
}
