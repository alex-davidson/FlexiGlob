namespace FlexiGlob
{
    public abstract class RootSegment
    {
        public string Token { get; }

        protected RootSegment(string token)
        {
            Token = token;
        }

        public override string ToString() => Token;
    }
}
