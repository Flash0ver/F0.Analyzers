# Use Pattern Matching to check for null

CodeRefactoringProvider: [UsePatternMatchingNullCheckInsteadOfComparisonWithNull.cs](../../source/production/F0.Analyzers/CodeAnalysis/CodeFixes/UsePatternMatchingNullCheckInsteadOfComparisonWithNull.cs)

|            |                                    |
|------------|------------------------------------|
| Title      | Use constant 'null' pattern        |
| Fixes      | [F01001][F01001], [F01002][F01002] |
| Language   | C# 9.0 or greater                  |
| Applies to | `[0.7.0,)`                         |

## Summary

Replaces `null` checks via potentially user-overloaded operators and potentially user-overridden methods with either the `is null` or the `is not null` pattern.

## Remarks

Fixes [F01001][F01001]: perform an actual `null` check via _pattern matching_, which forces a `null` check even when the equality operators are overloaded or a well-known equality method is overridden.

Fixes [F01002][F01002]: a more clear way to check for `null` via _pattern matching_.

## Example

Before:
```cs
public void NullCheck(Record instance, IEquatable<Record> equatable, IEqualityComparer<Record> comparer)
{
    _ = instance == null;
    _ = instance != null;
    _ = (object)instance == null;
    _ = (object)instance != null;

    _ = Object.Equals(instance, null);
    _ = !Object.Equals(instance, null);
    _ = Object.ReferenceEquals(instance, null);
    _ = !Object.ReferenceEquals(instance, null);

    _ = instance.Equals(null);
    _ = !instance.Equals(null);
    _ = equatable.Equals(null);
    _ = !equatable.Equals(null);
    _ = comparer.Equals(instance, null);
    _ = !comparer.Equals(instance, null);
}
```

After:
```cs
public void NullCheck(Record instance, IEquatable<Record> equatable, IEqualityComparer<Record> comparer)
{
    _ = instance is null;
    _ = instance is not null;
    _ = instance is null;
    _ = instance is not null;

    _ = instance is null;
    _ = instance is not null;
    _ = instance is null;
    _ = instance is not null;

    _ = instance is null;
    _ = instance is not null;
    _ = equatable is null;
    _ = equatable is not null;
    _ = instance is null;
    _ = instance is not null;
}
```

## See also

- [C# version 7.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-7)
- [C# version 9.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9)

## History

- [0.7.1](../../CHANGELOG.md#v071-2021-06-13)
- [0.7.0](../../CHANGELOG.md#v070-2021-05-11)


[F01001]: ../diagnostics/F0100x.md#F01001
[F01002]: ../diagnostics/F0100x.md#F01002
