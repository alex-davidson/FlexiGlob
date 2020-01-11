using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob.Parsing
{
    internal class ParseContext
    {
        public string Pattern { get; }

        public ParseContext(string globPattern)
        {
            this.Pattern = globPattern;
        }

        public GlobFormatException CreateError(string message) => CreateError(message, 0);
        public GlobFormatException CreateError(string message, Token token) => CreateError(message, token.Start);
        public GlobFormatException CreateError(string message, int start) => throw new GlobFormatException(message, Pattern, start);

        public string GetOriginalText(Token token) => Pattern.Range(token.Start, token.End);
        public string GetOriginalText(IList<Token> tokens) => Pattern.Range(tokens.First().Start, tokens.Last().End);
    }
}
