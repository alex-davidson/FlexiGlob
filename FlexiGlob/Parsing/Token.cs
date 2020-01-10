namespace FlexiGlob.Parsing
{
    internal struct Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }

        public static Token Literal(string value, int start, int end) =>
            new Token { Type = TokenType.Literal, Value = value, Start = start, End = end };
        public static Token Separator(int position) =>
            new Token { Type = TokenType.Separator, Value = "", Start = position, End = position + 1 };
        public static Token MatchAny(int start, int end) =>
            new Token { Type = TokenType.MatchAny, Start = start, End = end };
        public static Token MatchAnyRecursive(int start, int end) =>
            new Token { Type = TokenType.MatchAnyRecursive, Start = start, End = end };
        public static Token MatchRange(string characters, int start, int end) =>
            new Token { Type = TokenType.MatchRange, Value = characters, Start = start, End = end };
        public static Token MatchRangeInverted(string characters, int start, int end) =>
            new Token { Type = TokenType.MatchRangeInverted, Value = characters, Start = start, End = end };
        public static Token MatchSingle(int start, int end) =>
            new Token { Type = TokenType.MatchSingle, Start = start, End = end };
    }
}
