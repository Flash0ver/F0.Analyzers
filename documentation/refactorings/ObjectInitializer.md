# Object Initializer

CodeRefactoringProvider: [ObjectInitializer.cs](../../source/production/F0.Analyzers/CodeAnalysis/CodeRefactorings/ObjectInitializer.cs)

|            |                           |
|------------|---------------------------|
| Title      | Create Object Initializer |
| Language   | C# 3.0 or greater         |
| Applies to | `[0.1.0,)`                |

## Summary

This code refactoring adds an object initializer, assigning matching `locals`, matching `members`, or the `default value expression` to all publicly/internally settable instance fields and properties, both declared and inherited.
- C# 7.0 or lower: `default operator`
- C# 7.1 or greater: `default literal`

## Remarks

C# language features
- The `default operator` feature is available since _C# 2.0_.
- The `object initializer` feature is available since _C# 3.0_.
- The `default literal` feature is available since _C# 7.1_.
- The `target-typed new expressions` feature is available since _C# 9.0_.

This code refactoring (titled `Create Object Initializer`) creates an object initializer from any explicit or implicit object creation expression (`new operator`) and initializes all instance members that are both accessible and mutable.

Assignable members (declared or inherited) must be accessible from the location of the object initializer:
- `public` (unrestricted)
- `internal` (within same assembly or friend assemblies)
- `protected internal` (within same assembly or friend assemblies)

Candidate values are determined by this matching behavior:
- _locals_ and _members_, which exist in the current context
- same _type_
- equivalent _identifier name_ (case-insensitive comparison)

Additionally, the **.NET Runtime naming conventions** are observed, where fields may be prefixed:
- `_`
- `s_`
- `t_`

Value precedence
1. local variable/constant
2. parameter
3. field
4. property
5. default value expression

A match is unambiguous, when it is the only candidate in its precedence level.
The unambiguous match with the highest precedence gets assigned in the object initializer syntax.
When no unambiuous match exists, the default value is assigned.
If the csproj's `LangVersion` is C# 7.1 or greater, then the `default literal` is assigned, else the `default operator` is assigned.

For language versions lower than C# 3.0 no code refactoring is registered.

## Example

Before:
```cs
class MyClass
{
    float Item3;
    string Item4 { get; set; }

    ValueTuple<bool, int, float, string, object> MyMethod(int item2)
    {
        bool item1 = true;
        return new ValueTuple<bool, int, float, string, object>();
    }
}
```

After:
```cs
class MyClass
{
    float Item3;
    string Item4 { get; set; }

    ValueTuple<bool, int, float, string, object> MyMethod(int item2)
    {
        bool item1 = true;
        return new ValueTuple<bool, int, float, string, object>()
        {
            Item1 = item1,
            Item2 = item2,
            Item3 = Item3,
            Item4 = Item4,
            Item5 = default
        };
    }
}
```

## See also

- [Object initializers](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers#object-initializers)
- [default value expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/default)
- [new operator](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/new-operator)
- [target-typed new expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/target-typed-new)
- [Friend assemblies](https://docs.microsoft.com/en-us/dotnet/standard/assembly/friend)
- [C# version 2.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-20)
- [C# version 3.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-30)
- [C# version 7.1](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-71)
- [C# version 9.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-90)
- [C# Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)

## History

- [0.8.0](../../CHANGELOG.md#v080-2021-10-13)
- [0.6.1](../../CHANGELOG.md#v061-2021-04-18)
- [0.6.0](../../CHANGELOG.md#v060-2020-11-21)
- [0.5.0](../../CHANGELOG.md#v050-2020-09-29)
- [0.4.1](../../CHANGELOG.md#v041-2020-07-11)
- [0.4.0](../../CHANGELOG.md#v040-2020-06-26)
- [0.2.0](../../CHANGELOG.md#v020-2020-05-21)
- [0.1.0](../../CHANGELOG.md#v010-2020-05-14)
