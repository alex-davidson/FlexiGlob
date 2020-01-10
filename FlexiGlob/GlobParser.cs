using System;
using System.Collections.Generic;
using System.Linq;
using FlexiGlob.Parsing;

namespace FlexiGlob
{
    /// <summary>
    /// Parse a glob into an optional root locator and a sequence of path segment matchers.
    /// </summary>
    /// <remarks>
    /// In the name of simplicity, and consistency across platforms, various edge cases are either not handled or are
    /// rejected outright.
    ///
    /// * Repeated path separators are rejected, unless they specify the start of a UNC path.
    /// * The only directory separator supported is the forward slash, /
    /// * Supported wildcard sequences are **, *, ?, [...], [!...]
    /// * The backslash \ is the escape character, not a directory separator.
    /// * Caller-defined variables are denoted in patterns with braces around their name, eg. {some_var}.
    /// * A glob which begins with segment consisting of a single character and a colon ':' (whitespace ignored) will always
    ///   be parsed as a Windows drive letter, even if the platform is not Windows.
    /// * The '..' relative path segment is rejected.
    /// * The '.' path segment is rejected, *except* when it appears at the very start of the glob ('./') in which
    ///   case the subsequent segment will not be parsed as a root locator. This may be used to get around Windows drive
    ///   letter identification.
    /// * Whitespace may be trimmed arbitrarily when looking for a root locator.
    /// * Escape sequences in root locators (eg. UNC machine or share names) are not supported.
    ///
    /// So, for sanity's sake, avoid using any of the following in file or directory names:
    /// * Leading or trailing whitespace.
    /// * Any special character: *, ?, [...], {...}, \, /.
    /// * A single character and a colon ':'.
    ///
    /// Since these are reasonable rules to live by anyway, I do not intend to ever fix this state of affairs.
    /// </remarks>
    public class GlobParser
    {
        public List<GlobVariable> Variables { get; } = new List<GlobVariable>();

        public Glob Parse(string pattern)
        {
            if (pattern.TrimStart() != pattern) throw new ArgumentException("Leading whitespace is not permitted.", nameof(pattern));

            var rootContext = new ParseContext(pattern);
            var root = TryParseRoot(rootContext, out var offset);

            var pathPattern = pattern.Substring(offset);

            if (pathPattern == "") return new Glob(root, new Segment[0]);

            var context = new ParseContext(pathPattern);
            var tokens = new GlobPathTokeniser().Tokenise(context).ToArray();
            var segments = new GlobPathParser(Variables.ToArray()).Parse(context, tokens).ToArray();

            return new Glob(root, segments);
        }

        public T ParseSingleSegment<T>(string pattern) where T : Segment
        {
            if (pattern == "") throw new ArgumentException("Pattern is empty.", nameof(pattern));

            var context = new ParseContext(pattern);
            var tokens = new GlobPathTokeniser().Tokenise(context).ToArray();
            var segments = new GlobPathParser(Variables.ToArray()).Parse(context, tokens).ToArray();

            if (segments.Length != 1) throw new ArgumentException($"Pattern parsed as {segments.Length} segments.", nameof(pattern));
            return segments.Single() as T ?? throw new ArgumentException($"Pattern parsed as a segment of type {segments.Single().GetType()} segments.", nameof(pattern));
        }

        private static RootSegment? TryParseRoot(ParseContext context, out int offset)
        {
            offset = 0;
            var index = context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator);
            if (index < 0) return null;
            if (index == 0)
            {
                if (context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator, 1) == 1)
                {
                    // Probable UNC path.
                    return ParseUNCRoot(context, out offset);
                }
                // Unix rooted path.
                offset = 1;
                return new LocalRootSegment();
            }
            var initialSegment = context.Pattern.Substring(0, index).Trim();
            if (initialSegment == ".")
            {
                AssertNoRepeatedSeparators(context, index);
                offset = index + 1;
                return null;
            }
            if (initialSegment.Length == 2 && initialSegment[1] == ':')
            {
                // Probable Windows drive letter.
                AssertNoRepeatedSeparators(context, index);
                offset = index + 1;
                return new LocalRootSegment(initialSegment);
            }

            AssertNoRepeatedSeparators(context, 0);
            return null;
        }

        private static void AssertNoRepeatedSeparators(ParseContext context, int start)
        {
            var index = context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator, start);
            while (index >= 0)
            {
                var nextIndex = context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator, index + 1);
                if (nextIndex == index + 1) throw context.CreateError("Repeated path separators are invalid.", index);
                index = nextIndex;
            }
        }

        /// <summary>
        /// These are not strictly valid in machine names, and may indicate a mis-typed or mis-parsed glob.
        /// </summary>
        private static readonly char[] invalidMachineNameCharacters = { ':' };
        /// <summary>
        /// These are not strictly valid in share names, and may indicate a mis-typed or mis-parsed glob.
        /// </summary>
        private static readonly char[] invalidShareNameCharacters = { ':' };

        private static RootSegment ParseUNCRoot(ParseContext context, out int offset)
        {
            var endOfMachine = context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator, 2);
            if (endOfMachine < 0) throw context.CreateError("UNC path must specify a share.");

            var machine = context.Pattern[2..endOfMachine];
            ValidateMachineName(context, machine, 2);

            var startOfShare = endOfMachine + 1;
            var endOfShare = context.Pattern.IndexOf(GlobPathTokeniser.SegmentSeparator, endOfMachine + 1);
            if (endOfShare < 0)
            {
                var share = context.Pattern[startOfShare..].Trim();
                ValidateShareName(context, share, startOfShare);
                // No remaining path. No possibility of repeated separators.
                offset = context.Pattern.Length;
                return new UNCRootSegment(context.Pattern, machine, share);
            }
            else
            {
                var share = context.Pattern[startOfShare..endOfShare].Trim();
                ValidateShareName(context, share, startOfShare);
                AssertNoRepeatedSeparators(context, endOfShare);
                offset = endOfShare + 1;
                return new UNCRootSegment(context.Pattern[..endOfShare], machine, share);
            }
        }

        private static void ValidateMachineName(ParseContext context, string machine, int startOfMachine)
        {
            if (string.IsNullOrEmpty(machine)) throw context.CreateError("UNC machine name must be specified.", startOfMachine);
            var invalidMachineNameCharacterIndex = machine.IndexOfAny(invalidMachineNameCharacters);
            if (invalidMachineNameCharacterIndex >= 0) throw context.CreateError("UNC machine name contains invalid characters.", startOfMachine + invalidMachineNameCharacterIndex);
        }

        private static void ValidateShareName(ParseContext context, string share, int startOfShare)
        {
            if (string.IsNullOrEmpty(share)) context.CreateError("UNC path must specify a share.", startOfShare);
            var invalidShareNameCharacterIndex = share.IndexOfAny(invalidShareNameCharacters);
            if (invalidShareNameCharacterIndex >= 0) context.CreateError("UNC share name contains invalid characters.", startOfShare + invalidShareNameCharacterIndex);
        }
    }
}
