using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FlexiGlob.Parsing
{
    internal class GlobPathTokeniser
    {
        public static readonly char SegmentSeparator = '/';
        public static readonly char EscapePrefix = '\\';

        public IEnumerable<Token> Tokenise(ParseContext context)
        {
            var reader = new GlobPatternReader(context.Pattern);
            do
            {
                var start = reader.Position;
                var text = ReadLiteral(reader, beginSpecialChars);
                if (text.Length > 0) yield return Token.Literal(text, start, reader.Position);
                if (reader.IsEndOfPattern) break;
                yield return TryReadSpecial(context, reader);
            }
            while (!reader.IsEndOfPattern);
        }

        public bool ContainsSpecialCharacters(string globPattern)
        {
            return globPattern.IndexOfAny(wildcardChars) >= 0;
        }

        public string GetPlainPrefix(string globPattern)
        {
            var end = globPattern.IndexOfAny(wildcardChars);
            if (end < 0) return globPattern;
            return globPattern.Substring(0, end);
        }

        private static readonly char[] beginSpecialChars = { '*', '[', '{', '?', EscapePrefix, SegmentSeparator };
        private static readonly char[] wildcardChars = { '*', '[', '?', ']', '{', '}' };
        private static readonly Regex rxRange = new Regex(@"^!?(?!!)([^-]-[^-]|[^-]+)$", RegexOptions.Compiled);

        private string ReadLiteral(GlobPatternReader reader, char[] terminators)
        {
            return reader.ReadUntilAny(terminators);
        }

        private Token TryReadSpecial(ParseContext context, GlobPatternReader reader)
        {
            var start = reader.Position;
            if (reader.TryRead(SegmentSeparator))
            {
                return Token.Separator(start);
            }
            if (reader.TryRead(EscapePrefix))
            {
                if (!TryReadEscapeCode(reader, out var escaped)) throw context.CreateError($"Invalid escape code: {reader.GetSnippet(start, reader.Position)}", start);
                return Token.Literal(escaped, start, reader.Position);
            }
            if (reader.TryRead('*'))
            {
                if (reader.TryRead('*'))
                {
                    return Token.MatchAnyRecursive(start, reader.Position);
                }
                return Token.MatchAny(start, reader.Position);
            }
            if (reader.TryRead('?'))
            {
                return Token.MatchSingle(start, reader.Position);
            }
            if (reader.TryRead('['))
            {
                var range = reader.ReadUntilAny(wildcardChars);
                if (!reader.TryRead(']'))
                {
                    throw reader.IsEndOfPattern
                        ? context.CreateError($"Range at position {start} is not closed: {reader.GetSnippet(start, range.Length + 1)}", start)
                        : context.CreateError($"Range at position {start} contains an invalid character: {reader.GetSnippet(5)}", start);
                }
                if (range.Length == 0) throw context.CreateError($"Range at position {start} is empty: {reader.GetSnippet(start, 10)}", start);
                var m = rxRange.Match(range);
                if (range[0] == '!')
                {
                    if (range.Length == 1) throw context.CreateError($"Range at position {start} is empty: {reader.GetSnippet(start, 10)}", start);
                }
                if (!m.Success) throw context.CreateError($"Range at position {start} is not valid: {reader.GetSnippet(start, range.Length + 2)}", start);

                var rangeCharacters = m.Groups[1].Value;
                if (range[0] == '!') return Token.MatchRangeInverted(rangeCharacters, start, reader.Position);
                return Token.MatchRange(rangeCharacters, start, reader.Position);
            }
            if (reader.TryRead('{'))
            {
                var variableName = reader.ReadUntilAny(wildcardChars);
                if (!reader.TryRead('}'))
                {
                    throw reader.IsEndOfPattern
                        ? context.CreateError($"Variable reference at position {start} is not closed: {reader.GetSnippet(start, variableName.Length + 1)}", start)
                        : context.CreateError($"Variable reference at position {start} contains an invalid character: {reader.GetSnippet(5)}", start);
                }
                return Token.MatchVariable(variableName, start, reader.Position);
            }
            throw context.CreateError($"Unable to parse wildcard expression at position {start}: {reader.GetSnippet(start, 10)}", start);
        }

        private bool TryReadEscapeCode(GlobPatternReader reader, out string escaped)
        {
            escaped = "";
            var firstCharacter = reader.ReadOne();
            if (firstCharacter == null) return false;

            escaped = firstCharacter.ToString();

            return true;
        }

        private class GlobPatternReader
        {
            public string Pattern { get; }
            public int Position { get; private set; }
            public bool IsEndOfPattern => RemainingCharacters <= 0;
            private int RemainingCharacters => Pattern.Length - Position;

            public GlobPatternReader(string globPattern)
            {
                this.Pattern = globPattern;
            }

            public string ReadUntilAny(char[] terminators)
            {
                var text = ReadAheadUntilAny(terminators);
                Position += text.Length;
                return text;
            }

            public string ReadAheadUntilAny(char[] terminators)
            {
                var terminatorIndex = Pattern.IndexOfAny(terminators, Position);
                if (terminatorIndex < 0) return Pattern.Substring(Position);
                var count = terminatorIndex - Position;
                if (count <= 0) return "";
                return Pattern.Substring(Position, count);
            }

            public char? ReadOne()
            {
                if (RemainingCharacters < 1) return null;
                var character = Pattern[Position];
                Position += 1;
                return character;
            }

            public bool TryRead(char wildcard)
            {
                if (RemainingCharacters < 1) return false;
                if (Pattern[Position] != wildcard) return false;
                Position += 1;
                return true;
            }

            public string GetSnippet(int count) => GetSnippet(Position, count);
            public string GetSnippet(int start, int count)
            {
                var available = Pattern.Length - start;
                if (count >= available) return Pattern.Substring(start, available);
                return Pattern.Substring(start, count) + "...";
            }
        }
    }
}
