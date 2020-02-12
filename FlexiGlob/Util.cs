using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FlexiGlob
{
    internal static class Util
    {
        /// <summary>
        /// Find longest common prefix of a list of strings.
        /// </summary>
        internal static string LongestCommonPrefix(List<string> values, bool caseSensitive)
        {
            if (values.Count == 0) throw new ArgumentException("Cannot find common prefixes in an empty list.", nameof(values));
            if (values.Count == 1) return values[0];

            if (caseSensitive)
            {
                return LongestCommonPrefixInternal(values, CompareCaseSensitive);
            }
            else
            {
                return LongestCommonPrefixInternal(values, CompareCaseInsensitive);
            }
        }

        private static bool CompareCaseSensitive(char a, char b) => a == b;
        private static bool CompareCaseInsensitive(char a, char b) => char.ToUpper(a) == char.ToUpper(b);

        private static string LongestCommonPrefixInternal(List<string> values, Func<char, char, bool> areEqual)
        {
            var prefix = values[0];
            var length = prefix.Length;
            for (var i = 1; i < values.Count; i++)
            {
                length = Math.Min(length, values[i].Length);
                if (length == 0) return "";
                if (!areEqual(prefix[0], values[i][0])) return "";
                for (var j = 0; j < length; j++)
                {
                    if (!areEqual(prefix[j], values[i][j]))
                    {
                        length = j;
                        break;
                    }
                }
            }
            return prefix.RangeTo(length);
        }

        internal static string ToHexString(byte[] bytes)
        {
            const string lookup = "0123456789abcdef";
            var chars = new char[2 * bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                var j = i * 2;
                chars[j] = lookup[bytes[i] & 0xf];
                chars[j + 1] = lookup[(bytes[i] >> 4) & 0xf];
            }
            return new string(chars);
        }
    }
}
