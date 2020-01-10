using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FlexiGlob.Parsing
{
    internal class GlobPathParser
    {
        public IEnumerable<Segment> Parse(ParseContext context, Token[] tokens)
        {
            // Assumptions:
            // * Caller checked for repeated separators already.
            // * First token is not a separator.
            var segmentTokens = new List<Token>(10);
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Separator)
                {
                    Debug.Assert(segmentTokens.Count > 0);
                    yield return CreateSegment(context, segmentTokens);
                    segmentTokens.Clear();
                }
                else
                {
                    segmentTokens.Add(token);
                }
            }
            if (segmentTokens.Count > 0) yield return CreateSegment(context, segmentTokens);
        }

        private Segment CreateSegment(ParseContext context, List<Token> tokens)
        {
            var index = tokens.FindIndex(t => t.Type == TokenType.MatchAnyRecursive);
            if (index >= 0)
            {
                if (tokens.Count > 1)
                {
                    throw context.CreateError("Wildcard '**' cannot share a segment with other matchers.", tokens[index].Start);
                }
                return new WildcardMultiSegment();
            }

            var leadingLiteralText = string.Concat(tokens.TakeWhile(t => t.Type == TokenType.Literal).Select(t => t.Value));
            if (tokens.All(t => t.Type == TokenType.Literal))
            {
                ValidateLiteralSegment(context, leadingLiteralText, tokens[0].Start);
                return new FixedSegment(leadingLiteralText);
            }

            var regex = CompileRegex(tokens);
            var original = context.GetOriginalText(tokens);
            return new WildcardSegment(original, regex, leadingLiteralText);
        }

        private void ValidateLiteralSegment(ParseContext context, string literal, int start)
        {
            if (literal.Trim() == ".") throw context.CreateError("Relative segment '.' cannot be used within a globbed path.", start);
            if (literal.Trim() == "..") throw context.CreateError("Relative segment '..' cannot be used within a globbed path.", start);
        }

        private string CompileRegex(List<Token> tokens)
        {
            var builder = new StringBuilder(32);
            builder.Append("^");
            foreach (var token in tokens)
            {
                AddRegexForToken(builder, token);
            }
            builder.Append("$");
            return builder.ToString();
        }

        private void AddRegexForToken(StringBuilder builder, Token token)
        {
            switch (token.Type)
            {
                case TokenType.Literal:
                    builder.Append(Regex.Escape(token.Value));
                    break;

                case TokenType.Separator:
                    throw new ArgumentException("Cannot create regex for TokenType.Separator.");

                case TokenType.MatchAny:
                    builder.Append(".*");
                    break;
                case TokenType.MatchAnyRecursive:
                    throw new ArgumentException("Cannot create regex for TokenType.MatchAnyRecursive.");

                case TokenType.MatchRange:
                    builder.Append('[');
                    builder.Append(Regex.Escape(token.Value));
                    builder.Append(']');
                    break;

                case TokenType.MatchRangeInverted:
                    builder.Append('[');
                    builder.Append('^');
                    builder.Append(Regex.Escape(token.Value));
                    builder.Append(']');
                    break;

                case TokenType.MatchSingle:
                    builder.Append(".");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
