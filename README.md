# FlexiGlob

*Flexible globbing library for .NET*

## Why?

There are a fair few .NET globbing libraries out there, but they are usually optimised for speed at the expense of tweakability.

Features I needed:
* Ability to control exactly how the filesystem is traversed during matching, in order to do things like transparently treating ZIP archives as folders.
* Extra syntax for defining variables, which are populated from regions of the matched path during traversal,
* Control over case-sensitivity.

Plus, writing a globbing library was a lot of fun.

Performance was not a primary goal, but some effort has been made to avoid excessive use of regexen and enable future optimisations.

## Dependencies

* Built upon .NET Standard 2.1, so it should be compatible with .NET Core 3 and any later versions of the same.
* No additional library dependencies.

It would be convenient to be able to use this in .NET Framework projects too so I might reduce the .NET Standard version dependency at some point.

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

For simple use cases, GlobMatchEnumerator should be suitable.
1. Parse the glob expression.
1. Use the Root property of the Glob to determine the starting point of the search. This may be null, a local drive, or a network drive.
1. Create a FileSystemHierarchy.
1. Use a GlobMatchEnumerator to enumerate matches.

```
var glob = new GlobParser().Parse(globString);
var directoryInfo = FindStartingDirectory(glob.Root);   // You need to implement this according to your needs.
var filesystem = new FileSystemHierarchy(directoryInfo, caseSensitive: true);
foreach (var match in new GlobMatchEnumerator(glob).EnumerateMatches(filesystem))
{
    // For example:
    Console.WriteLine(match.Item.FullName);
    // Extra information is available via Details:
    var variables = match.Details.GetVariables();
}
```

### Advanced

The primary API of the library consists of:
* Glob: the parsed glob expression, consisting of an optional root and a list of segments.
* GlobParser: parses and validates a glob string into a Glob object.
* GlobMatchFactory: Creates a 'starting point' IGlobMatch from a Glob.
* IGlobMatch: filters child path segments and provides access to any variables encountered.

An IGlobMatch is immutable and represents the state of the matcher. Calling MatchChild with a path segment returns a new IGlobMatch.

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

## License

FlexiGlob is released under the [Unlicense](LICENSE). Do whatever you like with it.
