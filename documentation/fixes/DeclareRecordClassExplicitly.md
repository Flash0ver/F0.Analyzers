# Prefer explicit reference type records

CodeFixProvider: [DeclareRecordClassExplicitly.cs](../../source/production/F0.Analyzers/CodeAnalysis/CodeFixes/DeclareRecordClassExplicitly.cs)

|            |                                                              |
|------------|--------------------------------------------------------------|
| Title      | Explicitly add class keyword to reference record declaration |
| Fixes      | [F02001][F02001]                                             |
| Language   | C# 10.0 or greater                                           |
| Applies to | `[0.9.0,)`                                                   |

## Summary

Adds the optional `class` keyword to _reference record_ declarations to add clarity for readers.

## Remarks

Both a `record` and a `record class` declare a _reference type_ and are semantically equal.
The `class` keyword is optional, distinguishing these types from `record struct` and `readonly record struct` declarations, both value types with the semantic difference of immutability.

## Example

Before:
```cs
public record Record(int Number, string Text);
```

After:
```cs
public record class Record(int Number, string Text);
```

## See also

- [What's new in C# 10 - Record structs](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#record-structs)

## History

- [0.9.0](../../CHANGELOG.md#v090-2022-02-04)


[F02001]: ../diagnostics/F02001.md
