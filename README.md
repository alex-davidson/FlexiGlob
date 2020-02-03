# FlexiGlob

*Flexible globbing library for .NET*

* FlexiGlob is released under the [Unlicense](LICENSE). Do whatever you like with it.
* [Changelog](CHANGELOG.md)

## Why?

There are a fair few .NET globbing libraries out there, but they are usually optimised for speed at the expense of tweakability.

Features I needed:
* Ability to control exactly how the filesystem is traversed during matching, in order to do things like transparently treating ZIP archives as folders.
* Extra syntax for defining variables, which are populated from regions of the matched path during traversal,
* Control over case-sensitivity.

Plus, writing a globbing library was a lot of fun.

Performance was not originally a primary goal, but some effort has been made to avoid excessive use of regexen and enable future optimisations, and once I start using the profiler I find it hard to stop.

## Dependencies

* Targets .NET Standard 1.3 and 2.1, therefore it should work with .NET Framework 4.6 and later, and all versions of .NET Core.
* No additional library dependencies.

## Supported Glob Syntax

* `/`: Path separator, even on Windows.
* `\`: Escape character, not a path separator. Typically used to force special characters such as `*` to be matched literally instead.
* `*`: Matches zero or more characters, but only within a path segment, eg. `a/*z/c` matches `a/bz/c` or `a/dz/c` but not `a/b/z/c`.
* `**`: Matches zero or more entire path segments, eg. `**/test` matches `a/b/c/test` or `test`
* `?`: Matches a single character, eg. `abc?` matches `abcd` and `abcz` but not `abc` or `abcde`.
* `[...]`: Matches any single character in the specified range, according to the same rules as the .NET Regex class, eg. `[a-cz]` matches `a`, `b`, `c` or `z`.
* `[!...]`: Matches any single character not in the specified range, according to the same rules as the .NET Regex class, eg. `[!a-cz]` matches any character except `a`, `b`, `c` or `z`.
* `{...}`: Matches a custom, named pattern and stores the text in a variable.

## Usage

Generally speaking, this library does not operate on absolute paths, or automatically handle 'root' specifiers in globs.

The parser produces a Glob object which has Root and Segments properties. The consuming code is expected to deal with the Root as necessary for its own use cases. This because:

* Handling all the edge cases properly is Really Hard.
* Across all possible use cases for this library, pretty much every edge case will arise.
* Any individual use case only needs to handle a smaller, simpler set of edge cases.

There are three main 'helper' classes which aim to deal with the Segments property, matching against relative paths. They all support case-sensitive and case-insensitive matching.
* GlobMatcher tests globs against individual strings and string arrays.
* GlobMatchEnumerator applies a glob to a hierarchy, looking for matches.
* MultiGlobMatchEnumerator applies multiple inclusion and exclusion rules to a hierarchy.

The concept of a hierarchy of string-named objects is abstracted by `IGlobMatchableHierarchy<T>`.
* The typical example is `FileSystemHierarchy`, consisting of `FileSystemInfo` objects.
* A `GlobMatchableList<T>` base type is provided for matching against eg. lists of names.
* Implementing a RegistryKeyHierarchy should be fairly straightforward.

The GlobMatchEnumerator and MultiGlobMatchEnumerator try to skip entire subtrees when a match is not possible.

### I want to: check if a single glob matches one or more relative paths, which I already have in a list.

```
// Parse the glob expression and create a GlobMatcher.
var glob = new GlobParser().Parse(globString);
var matcher = new GlobMatcher(glob, caseSensitive: true);
// Split the path into segments, according to the rules of the platform.
var somePathSegments = "some/path/here".Split('/');
// Test for a match.
return matcher.IsMatch(somePath.Split('/'));
```

There is also a `Match` method, which returns an entire GlobMatch object for more advanced cases (see Variables below).

### I want to: find all paths inside a directory which are matched by a single glob.

```
// Parse the glob expression.
var glob = new GlobParser().Parse(globString);
var directoryInfo = FindStartingDirectory(glob.Root);   // You need to implement this according to your needs.
// Create a FileSystemHierarchy from the starting directory.
var filesystem = new FileSystemHierarchy(directoryInfo, caseSensitive: true);
// Use a GlobMatchEnumerator to enumerate matches.
foreach (var match in new GlobMatchEnumerator(glob).EnumerateMatches(filesystem))
{
    // For example:
    Console.WriteLine(match.Item.FullName);
    // Extra information is available via Details:
    var variables = match.Details.GetVariables();
    var relativePath = string.Join("/", match.Details.GetPathSegments());
}
```

### I want to: find all paths matching one glob which are not matched by another glob.

```
var parser = new GlobParser();
// Create a MultiGlobMatchEnumerator from your glob rules. Note that earlier rules take precedence.
// We want exclusions to override inclusions here, so we specify exclusions first.
var matchEnumerator = new MultiGlobMatchEnumerator()
    .Exclude(parser.Parse(excludeGlob))
    .Include(parser.Parse(includeGlob));
// Create a FileSystemHierarchy from the starting directory. Deciding where this is can be hard when
// multiple globs are involved. Maybe require all the globs to be relative, ie. Root == null?
var filesystem = new FileSystemHierarchy(startingDirectoryInfo, caseSensitive: true);
// Enumerate the matches.
foreach (var match in matchEnumerator.EnumerateMatches(filesystem))
{
    // For example:
    Console.WriteLine(match.Item.FullName);
    // Extra information is available via Details:
    var variables = match.Details.GetVariables();
    var relativePath = string.Join("/", match.Details.GetPathSegments());
}
```

You can mix inclusions and exclusions and they will be considered in the order they're specified. An earlier exclusion which matches will prevent any subsequent inclusions from producing a match.

It is also possible to do something like this:
```
var catchAll = parser.Parse("**");
var matchEnumerator = new MultiGlobMatchEnumerator()
    .Include(matchTheseRegardless)
    .Exclude(thenExcludeThese)
    .Include(matchTheseIfNotExcluded)
    .Include(catchAll);
foreach (var match in matchEnumerator.EnumerateMatches(filesystem))
{
    if (match.Details.First().Glob == catchAll)
    {
        log.Info($"File {match.Item.FullName} was found, but wasn't covered by any of the specified rules.");
        continue;
    }
    // For example:
    Console.WriteLine(match.Item.FullName);
}
```

### Advanced

The core API of the library consists of:
* Glob: the parsed glob expression, consisting of an optional root and a list of segments.
* GlobParser: parses and validates a glob string into a Glob object.
* GlobMatchFactory: Creates a 'starting point' GlobMatch from a Glob.
* GlobMatch: filters child path segments and provides access to any variables encountered.

A GlobMatch is immutable and represents the state of the matcher. Calling MatchChild with a path segment returns a new GlobMatch.

All of the simpler APIs are constructed from the above and usually work by iteratively or recursively mapping lists of GlobMatch.

### Variables

Variable patterns are defined by the application consuming this library. By default, none are configured.

They are useful if you want to extract data from the pathname as you traverse it. An example here might be a log cleanup tool, which extracts the date of a log file from its name.

Each variable has a name and a pattern, which is defined as a regex. For example:
```
var parser = new GlobParser
{
    Variables =
    {
        new GlobVariable("yyyy", @"\d{4}"),
        new GlobVariable("MM", @"\d{2}"),
        new GlobVariable("dd", @"\d{2}"),
    }
};
```

This would then allow a user to configure a glob like the following: `logs/{yyyy}/{MM}-{dd}/*.log`

When a path like `logs/2020/01-10/service.log` is matched, the IGlobMatch object will contain three variables:
* `yyyy`: `2020`
* `MM`: `01`
* `dd`: `10`

These can then be read and handled by the consuming code.
