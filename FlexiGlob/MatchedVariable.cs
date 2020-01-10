namespace FlexiGlob
{
    public struct MatchedVariable
    {
        public static readonly MatchedVariable[] None = new MatchedVariable[0];

        public MatchedVariable(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }

        public override string ToString() => $"{Name} = {Value}";
    }
}
