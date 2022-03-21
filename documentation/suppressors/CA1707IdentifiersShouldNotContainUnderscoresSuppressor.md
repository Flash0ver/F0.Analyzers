# Suppress CA1707 for test methods

DiagnosticSuppressor: [CA1707IdentifiersShouldNotContainUnderscoresSuppressor.cs](../../source/production/F0.Analyzers/CodeAnalysis/Suppressors/CA1707IdentifiersShouldNotContainUnderscoresSuppressor.cs)

|            |                  |
|------------|------------------|
| ID         | F0CA1707         |
| Suppresses | [CA1707][ca1707] |
| Applies to | `[0.10.0,)`       |

## Summary

Suppress warning [CA1707][ca1707] on test methods to allow the best practices naming standard for tests.

## Remarks

[CA1707][ca1707] reports a warning on non-field identifiers when containing underscores, i.e., a `_` character.

The [best practices for writing unit tests in .NET](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) suggest the following [naming standard](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#naming-your-tests):

> The name of your test should consist of three parts:
>
> - The name of the method being tested.
> - The scenario under which it's being tested.
> - The expected behavior when the scenario is invoked.

These parts should be separated by the _underscore_ (`_`) character.

[CA1707][ca1707] is suppressed only for _MSTest_, _NUnit_ and _xUnit.net_ test methods.
For regular methods and other identifiers, [CA1707][ca1707] is not suppressed.

### MSTest
A method is a _MSTest_ test method, when it has
- either the `TestMethodAttribute`
- or the `DataTestMethodAttribute`

and the containing _class_ is attributed with
- `TestClassAttribute`

from the _namespace_ `Microsoft.VisualStudio.TestTools.UnitTesting`.

### NUnit
A method is an _NUnit_ test method, when it has one of the following _attributes_ (Namespace: `NUnit.Framework`):
- `TestAttribute`
- `TestCaseAttribute`
- `TestCaseSourceAttribute`
- `CombinatorialAttribute`
- `PairwiseAttribute`
- `SequentialAttribute`
- `TheoryAttribute`

### xUnit.net
A method is a _xUnit.net_ test method, when it has one of the following _attributes_ (Namespace: `Xunit`):
- `FactAttribute`
- `TheoryAttribute`

## Example

```cs
using Xunit;

namespace My_Namespace; // Remove the underscores from namespace name 'My_Namespace'

public class My_Tests // Remove the underscores from type name My_Namespace.My_Tests
{
    [Fact]
    public void Given_When_Then() // Warning CA1707 suppressed
    {
        Assert.Equal(240, 0x_F0);
    }

    [Theory]
    [InlineData(0x_F0)]
    public void MethodUnderTest_Scenario_ExpectedResult(int value) // Warning CA1707 suppressed
    {
        Assert.Equal(240, value);
    }

    public int My_Property { get; set; } // Remove the underscores from member name My_Namespace.My_Tests.My_Property

    public void My_Method() // Remove the underscores from member name My_Namespace.My_Tests.My_Method()
        => throw null;

    public void MyMethod<Method_TypeParameter>() // On method My_Namespace.My_Tests.MyMethod<Method_TypeParameter>(), remove the underscores from generic type parameter name Method_TypeParameter
        => throw null;
}
```

## See also

- [Warning CA1707][ca1707]
- [MSTest](https://github.com/microsoft/testfx)
- [NUnit](https://github.com/nunit/nunit)
- [xUnit.net](https://github.com/xunit/xunit)

## History

- [0.10.0](../../CHANGELOG.md#v0100-2022-03-21)


  [ca1707]: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1707
