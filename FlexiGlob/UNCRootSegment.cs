namespace FlexiGlob
{
    public class UNCRootSegment : RootSegment
    {
        public string Machine { get; }
        public string Share { get; }

        public UNCRootSegment(string token, string machine, string share) : base(token)
        {
            Machine = machine;
            Share = share;
        }

        public override string ToString() => $"{Token} (machine {Machine}, share {Share})";
    }
}
