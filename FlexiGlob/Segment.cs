namespace FlexiGlob
{
    public abstract class Segment
    {
        public string Token { get; }
        public abstract string Prefix { get; }

        protected Segment(string token)
        {
            Token = token;
        }

        public override string ToString() => Token;
    }
}
