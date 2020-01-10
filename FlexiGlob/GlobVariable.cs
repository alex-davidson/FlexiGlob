namespace FlexiGlob
{
    public class GlobVariable
    {
        public GlobVariable(string name, string regexPattern)
        {
            Name = name;
            RegexPattern = regexPattern;
        }

        public string Name { get; }
        public string RegexPattern { get; }
    }
}
