using NUnit.Framework;

namespace FlexiGlob.UnitTests
{
    [TestFixture]
    public class GlobMatcherTests
    {
        [TestCase("Program Files", "Program Files")]
        [TestCase("Program Files/*", "Program Files/Typescript")]
        [TestCase("Program Files/Type*", "Program Files/Typescript")]
        [TestCase("Program Files/*script", "Program Files/Typescript")]
        [TestCase("**/*script", "Program Files/Typescript")]
        [TestCase("Program Files/**", "Program Files/Typescript/1.7")]
        [TestCase("Program Files/**/Typescript", "Program Files/Typescript")]
        public void Accepts(string glob, string path)
        {
            var matcher = new GlobMatcher(new GlobParser().Parse(glob), true);
            Assert.That(matcher.IsMatch(path.Split('/')), Is.True);
        }

        [TestCase("Program Files", "Program Files/Typescript")]
        [TestCase("Program Files/*", "Program Files")]
        [TestCase("Program Files/Typescript/**", "Program Files/Typescript")]
        public void Rejects(string glob, string path)
        {
            var matcher = new GlobMatcher(new GlobParser().Parse(glob), true);
            Assert.That(matcher.IsMatch(path.Split('/')), Is.False);
        }
    }
}
