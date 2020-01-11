using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlexiGlob
{
    internal static class Polyfill_NetStandard21
    {
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
        internal static IEnumerable<FileSystemInfo> GetChildrenMatchingPrefix(this FileSystemInfo item, string prefix, bool caseSensitive)
        {
            if (item is DirectoryInfo directory)
            {
                return directory.EnumerateFileSystemInfos(
                    prefix + "*",
                    new EnumerationOptions
                    {
                        MatchCasing = caseSensitive ? MatchCasing.CaseSensitive : MatchCasing.CaseInsensitive,
                        RecurseSubdirectories = false
                    });
            }
            return Enumerable.Empty<FileSystemInfo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Range(this string value, int start, int end) => value[start..end];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string RangeFrom(this string value, int start) => value[start..];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string RangeTo(this string value, int end) => value[..end];
#else
        internal static IEnumerable<FileSystemInfo> GetChildrenMatchingPrefix(this FileSystemInfo item, string prefix, bool caseSensitive)
        {
            if (item is DirectoryInfo directory)
            {
                if (caseSensitive)
                {
                    // This uses platform case-sensitivity. Since the prefix search is only used as an optimisation, on case-insensitive
                    // platforms the extra matches will get filtered out by the caller.
                    return directory.EnumerateFileSystemInfos(prefix + "*", SearchOption.TopDirectoryOnly);
                }
                else
                {
                    // If the platform is case-sensitive, we cannot get case-insensitive matches from this API. We must instead return
                    // everything and let the caller sort it out.
                    return directory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                }
            }
            return Enumerable.Empty<FileSystemInfo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryDequeue<T>(this Queue<T> queue, out T item)
        {
            item = default!;
            if (queue.Count == 0) return false;
            item = queue.Dequeue();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Range(this string value, int start, int end) => value.Substring(start, end - start);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string RangeFrom(this string value, int start) => value.Substring(start);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string RangeTo(this string value, int end) => value.Substring(0, end);
#endif
    }
}
