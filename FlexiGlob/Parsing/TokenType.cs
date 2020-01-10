namespace FlexiGlob.Parsing
{
    internal enum TokenType
    {
        Literal = 0,
        Separator = 1,
        MatchAny = 2,
        MatchAnyRecursive = 3,
        MatchRange = 4,
        MatchRangeInverted = 5,
        MatchSingle = 6,
    }
}
