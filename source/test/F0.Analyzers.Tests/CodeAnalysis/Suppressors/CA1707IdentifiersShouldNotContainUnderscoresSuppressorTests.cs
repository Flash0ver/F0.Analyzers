using System.Reflection;
using F0.CodeAnalysis.Suppressors;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;

namespace F0.Tests.CodeAnalysis.Suppressors;

public class CA1707IdentifiersShouldNotContainUnderscoresSuppressorTests
{
	private static readonly DiagnosticResult CA1707 = DiagnosticResult.CompilerWarning("CA1707").WithSeverity(DiagnosticSeverity.Hidden);

	[Fact]
	public void CA1707IdentifiersShouldNotContainUnderscoresSuppressor_CheckType()
		=> Verify.DiagnosticSuppressor<CA1707IdentifiersShouldNotContainUnderscoresSuppressor>().Type();

	[Theory]
	[InlineData(true, true, true, true)]
	[InlineData(true, false, false, false)]
	[InlineData(false, true, true, false)]
	public async Task ReportSuppressions_MSTest_SuppressForTestMethods(bool useTestClassAttribute, bool useTestMethodAttribute, bool useDataTestMethodAttribute, bool isSuppressed)
	{
		var code =
$@"using Microsoft.VisualStudio.TestTools.UnitTesting;

{(useTestClassAttribute ? "[TestClass]" : "")}
public class MSTest
{{
	{(useTestMethodAttribute ? "[TestMethod]" : "")}
	public void {{|#0:Given_When_Then|}}()
	{{
		Assert.AreEqual(240, 0x_F0);
	}}

	{(useDataTestMethodAttribute ? "[DataTestMethod]" : "")}
	[DataRow(0x_F0)]
	public void {{|#1:MethodUnderTest_Scenario_ExpectedResult|}}(int value)
	{{
		Assert.AreEqual(240, value);
	}}
}}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(isSuppressed),
			CA1707.WithLocation(1).WithIsSuppressed(isSuppressed),
		};

		await VerifyAsync(code, expected, typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute));
	}

	[Theory]
	[InlineData(true, false, true, false, true)]
	[InlineData(true, false, false, true, true)]
	[InlineData(false, false, true, false, true)]
	[InlineData(false, false, false, true, true)]
	[InlineData(false, true, true, false, true)]
	[InlineData(false, true, false, true, true)]
	public async Task ReportSuppressions_NUnit_SuppressForTestMethods(bool useTestFixtureAttribute, bool useTestAttribute, bool useTestCaseAttribute, bool useTestCaseSourceAttribute, bool isSuppressed)
	{
		var code =
$@"using NUnit.Framework;

{(useTestFixtureAttribute ? "[TestFixture]" : "")}
public class NUnitTests
{{
	[Test]
	public void {{|#0:Given_When_Then|}}()
	{{
		Assert.AreEqual(240, 0x_F0);
	}}

	{(useTestAttribute ? "[Test]" : "")}
	{(useTestCaseAttribute ? "[TestCase(0x_F0)]" : "")}
	{(useTestCaseSourceAttribute ? "[TestCaseSource(nameof(TestCaseSource))]" : "")}
	public void {{|#1:MethodUnderTest_Scenario_ExpectedResult|}}(int value)
	{{
		Assert.AreEqual(240, value);
	}}

	private static int[] TestCaseSource => new int[] {{ 0x_F0 }};
}}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(isSuppressed),
			CA1707.WithLocation(1).WithIsSuppressed(isSuppressed),
		};

		await VerifyAsync(code, expected, typeof(NUnit.Framework.TestAttribute));
	}

	[Fact]
	public async Task ReportSuppressions_NUnit_CombiningStrategyAttribute_SuppressForTestMethods()
	{
		var code =
@"using NUnit.Framework;

public class NUnitTests
{
	[Combinatorial]
	public void {|#0:Combinatorial_Attribute|}([Values(1, 2)] int number, [Values(""A"", ""B"")] string text)
	{
	}

	[Pairwise]
	public void {|#1:Pairwise_Attribute|}([Values(""a"", ""b"", ""c"")] string left, [Values(""+"", ""-"")] string middle, [Values(""x"", ""y"")] string right)
	{
	}

	[Sequential]
	public void {|#2:Sequential_Attribute|}([Values(1, 2, 3)] int number, [Values(""A"", ""B"")] string text)
	{
	}

	[Theory]
	public void {|#3:Theory_Attribute|}(int number)
	{
	}

	[DatapointSource]
	public int[] DatapointSource => new int[] { -1, 0, 1 };
}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(true),
			CA1707.WithLocation(1).WithIsSuppressed(true),
			CA1707.WithLocation(2).WithIsSuppressed(true),
			CA1707.WithLocation(3).WithIsSuppressed(true),
		};

		await VerifyAsync(code, expected, typeof(NUnit.Framework.CombiningStrategyAttribute));
	}

	[Fact]
	public async Task ReportSuppressions_xUnit_SuppressForTestMethods()
	{
		var code =
@"using Xunit;

public class xUnit
{
	[Fact]
	public void {|#0:Given_When_Then|}()
	{
		Assert.Equal(240, 0x_F0);
	}

	[Theory]
	[InlineData(0x_F0)]
	public void {|#1:MethodUnderTest_Scenario_ExpectedResult|}(int value)
	{
		Assert.Equal(240, value);
	}
}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(true),
			CA1707.WithLocation(1).WithIsSuppressed(true),
		};

		await VerifyAsync(code, expected, typeof(FactAttribute), typeof(Xunit.Assert));
	}

	[Fact]
	public async Task ReportSuppressions_NonTestMethods_NoOp()
	{
		var code =
@"using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Xunit;

[TestClass]
public class MSTest
{
	[TestMethod]
	public void {|#0:Given_When_Then|}() { }

	[DataTestMethod]
	[DataRow(0x_F0)]
	public void {|#1:MethodUnderTest_Scenario_ExpectedResult|}(int value) { }
}

[TestFixture]
public class NUnitTests
{
	[Test]
	public void {|#2:Given_When_Then|}() { }

	[TestCase(0x_F0)]
	[TestCaseSource(nameof(TestCaseSource))]
	public void {|#3:MethodUnderTest_Scenario_ExpectedResult|}(int value) { }

	private static int[] TestCaseSource => new int[] { 0x_F0 };
}

public class NUnitCombiningStrategyAttribute
{
	[Combinatorial]
	public void {|#4:Combinatorial_Attribute|}() { }

	[Pairwise]
	public void {|#5:Pairwise_Attribute|}() { }

	[Sequential]
	public void {|#6:Sequential_Attribute|}() { }

	[NUnit.Framework.Theory]
	public void {|#7:Theory_Attribute|}(int number) { }

	[DatapointSource]
	public int[] DatapointSource => new int[] { 0x_F0 };
}

public class xUnit
{
	[Fact]
	public void {|#8:Given_When_Then|}() { }

	[Xunit.Theory]
	[InlineData(0x_F0)]
	public void {|#9:MethodUnderTest_Scenario_ExpectedResult|}(int value) { }
}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(false),
			CA1707.WithLocation(1).WithIsSuppressed(false),
			CA1707.WithLocation(2).WithIsSuppressed(false),
			CA1707.WithLocation(3).WithIsSuppressed(false),
			CA1707.WithLocation(4).WithIsSuppressed(false),
			CA1707.WithLocation(5).WithIsSuppressed(false),
			CA1707.WithLocation(6).WithIsSuppressed(false),
			CA1707.WithLocation(7).WithIsSuppressed(false),
			CA1707.WithLocation(8).WithIsSuppressed(false),
			CA1707.WithLocation(9).WithIsSuppressed(false),
		};

		var attributes =
@"using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TestClassAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TestMethodAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DataTestMethodAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DataRowAttribute : Attribute
	{
		public DataRowAttribute(object data1) => throw null;
	}
}

namespace NUnit.Framework
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class TestFixtureAttribute : NUnitAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class TestAttribute : NUnitAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public class TestCaseAttribute : NUnitAttribute
	{
		public TestCaseAttribute(object? arg) => throw null;
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public class TestCaseSourceAttribute : NUnitAttribute
	{
		public TestCaseSourceAttribute(string sourceName) => throw null;
	}

	public abstract class NUnitAttribute : Attribute
	{
	}
}

namespace NUnit.Framework
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class CombinatorialAttribute : CombiningStrategyAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class PairwiseAttribute : CombiningStrategyAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class SequentialAttribute : CombiningStrategyAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class TheoryAttribute : CombiningStrategyAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public abstract class CombiningStrategyAttribute : NUnitAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class DatapointSourceAttribute : NUnitAttribute
	{
	}
}

namespace Xunit
{
	using Xunit.Sdk;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class FactAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TheoryAttribute : FactAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class InlineDataAttribute : DataAttribute
	{
		public InlineDataAttribute(params object[] data) => throw null;
	}
}

namespace Xunit.Sdk
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public abstract class DataAttribute : Attribute
	{
	}
}";

		await VerifyAsync(code, expected, attributes);
	}

	[Fact]
	public async Task ReportSuppressions_Alias_SuppressForTestMethods()
	{
		var code =
@"using System;
using MSTestAlias1 = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using MSTestAlias2 = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using MSTestAlias3 = Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute;
using MSTestAlias4 = Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute;
using NUnitAlias1 = NUnit.Framework.TestFixtureAttribute;
using NUnitAlias2 = NUnit.Framework.TestAttribute;
using NUnitAlias3 = NUnit.Framework.TestCaseAttribute;
using NUnitAlias4 = NUnit.Framework.TestCaseSourceAttribute;
using NUnitAlias5 = NUnit.Framework.CombinatorialAttribute;
using NUnitAlias6 = NUnit.Framework.PairwiseAttribute;
using NUnitAlias7 = NUnit.Framework.SequentialAttribute;
using NUnitAlias8 = NUnit.Framework.TheoryAttribute;
using XunitAlias1 = Xunit.FactAttribute;
using XunitAlias2 = Xunit.TheoryAttribute;
using XunitAlias3 = Xunit.InlineDataAttribute;

[MSTestAlias1]
public class MSTest
{
	[MSTestAlias2]
	public void {|#0:Alias_MSTest_TestMethodAttribute|}() { }

	[MSTestAlias3]
	[MSTestAlias4(0x_F0)]
	public void {|#1:Alias_MSTest_DataTestMethodAttribute|}(int value) { }
}

[NUnitAlias1]
public class NUnitTests
{
	[NUnitAlias2]
	public void {|#2:Alias_NUnit_TestAttribute|}() { }

	[NUnitAlias3(0x_F0)]
	public void {|#3:Alias_NUnit_TestCaseAttribute|}(int value) { }

	[NUnitAlias4(nameof(TestCaseSource))]
	public void {|#4:Alias_NUnit_TestCaseSourceAttribute|}(int value)
	{
	}

	private static int[] TestCaseSource => new int[] { 0x_F0 };
}

public class NUnitCombiningStrategyAttribute
{
	[NUnitAlias5]
	public void {|#5:Alias_NUnit_CombinatorialAttribute|}() { }

	[NUnitAlias6]
	public void {|#6:Alias_NUnit_PairwiseAttribute|}() { }

	[NUnitAlias7]
	public void {|#7:Alias_NUnit_SequentialAttribute|}() { }

	[NUnitAlias8]
	public void {|#8:Alias_NUnit_TheoryAttribute|}() { }
}

public class xUnit
{
	[XunitAlias1]
	public void {|#9:Alias_xUnit_FactAttribute|}() { }

	[XunitAlias2]
	[XunitAlias3(0x_F0)]
	public void {|#10:Alias_xUnit_TheoryAttribute|}(int value) { }
}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(true),
			CA1707.WithLocation(1).WithIsSuppressed(true),
			CA1707.WithLocation(2).WithIsSuppressed(true),
			CA1707.WithLocation(3).WithIsSuppressed(true),
			CA1707.WithLocation(4).WithIsSuppressed(true),
			CA1707.WithLocation(5).WithIsSuppressed(true),
			CA1707.WithLocation(6).WithIsSuppressed(true),
			CA1707.WithLocation(7).WithIsSuppressed(true),
			CA1707.WithLocation(8).WithIsSuppressed(true),
			CA1707.WithLocation(9).WithIsSuppressed(true),
			CA1707.WithLocation(10).WithIsSuppressed(true),
		};

		await VerifyAsync(code, expected, typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute), typeof(NUnit.Framework.TestAttribute), typeof(FactAttribute));
	}

	[Fact]
	public async Task ReportSuppressions_NonTestMethods_DoNotSuppress()
	{
		var code =
@"using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Xunit;

namespace {|#0:My_Namespace|};

[TestClass]
public class {|#1:MSTest_Tests|}
{
	[AssemblyInitialize]
	public static void {|#2:Assembly_Initialize|}(TestContext context) => throw null;

	[ClassInitialize]
	public static void {|#3:Class_Initialize|}(TestContext context) => throw null;

	[TestInitialize]
	public void {|#4:Test_Initialize|}() => throw null;

	[DataTestMethod]
	[DataRow(0x_F0)]
	public void TestMethod(int {|#5:method_parameter|}) => throw null;

	[TestCleanup]
	public void {|#6:Test_Cleanup|}() => throw null;

	[ClassCleanup]
	public static void {|#7:Class_Cleanup|}() => throw null;

	[AssemblyCleanup]
	public static void {|#8:Assembly_Cleanup|}() => throw null;
}

[TestFixture]
public class {|#9:NUnit_Tests|}
{
	[SetUp]
	public void {|#10:SetUp_Attribute|}() => throw null;

	[TestCase(0x_F0)]
	public void TestCase(int {|#11:method_parameter|}) => throw null;

	[TearDown]
	public void {|#12:TearDown_Attribute|}() => throw null;
}

public class {|#13:xUnit_Tests|} : IDisposable
{
	public xUnit_Tests() => throw null;

	[Xunit.Theory]
	[InlineData(0x_F0)]
	public void Theory(int {|#14:method_parameter|}) => throw null;

	public void Dispose() => throw null;
}

public class MyClass<{|#15:Type_TypeParameter|}>
{
	public int {|#16:My_Property|} { get; set; }

	public void MyMethod<{|#17:Method_TypeParameter|}>() => throw null;
}";

		var expected = new[]
		{
			CA1707.WithLocation(0).WithIsSuppressed(false),
			CA1707.WithLocation(1).WithIsSuppressed(false),
			CA1707.WithLocation(2).WithIsSuppressed(false),
			CA1707.WithLocation(3).WithIsSuppressed(false),
			CA1707.WithLocation(4).WithIsSuppressed(false),
			CA1707.WithLocation(5).WithIsSuppressed(false),
			CA1707.WithLocation(6).WithIsSuppressed(false),
			CA1707.WithLocation(7).WithIsSuppressed(false),
			CA1707.WithLocation(8).WithIsSuppressed(false),
			CA1707.WithLocation(9).WithIsSuppressed(false),
			CA1707.WithLocation(10).WithIsSuppressed(false),
			CA1707.WithLocation(11).WithIsSuppressed(false),
			CA1707.WithLocation(12).WithIsSuppressed(false),
			CA1707.WithLocation(13).WithIsSuppressed(false),
			CA1707.WithLocation(14).WithIsSuppressed(false),
			CA1707.WithLocation(15).WithIsSuppressed(false),
			CA1707.WithLocation(16).WithIsSuppressed(false),
			CA1707.WithLocation(17).WithIsSuppressed(false),
			CA1707.WithLocation(1, 1).WithIsSuppressed(false).WithArguments("My_Assembly"),
		};

		await VerifyAsync(code, "My_Assembly", expected, typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute), typeof(NUnit.Framework.TestAttribute), typeof(FactAttribute));
	}

	private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, params string[] additionalDocuments)
		=> Verify.DiagnosticSuppressor<CA1707IdentifiersShouldNotContainUnderscoresSuppressor, IdentifiersShouldNotContainUnderscoresAnalyzer>().SuppressionAsync(code, diagnostics, ReferenceAssemblies.NetStandard.NetStandard20, new string[][] { additionalDocuments });

	private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, params Type[] metadataReference)
		=> Verify.DiagnosticSuppressor<CA1707IdentifiersShouldNotContainUnderscoresSuppressor, IdentifiersShouldNotContainUnderscoresAnalyzer>().SuppressionAsync(code, diagnostics, ReferenceAssemblies.NetStandard.NetStandard20, metadataReference);

	private static Task VerifyAsync(string code, string assemblyName, DiagnosticResult[] diagnostics, params Type[] metadataReference)
		=> Verify.DiagnosticSuppressor<CA1707IdentifiersShouldNotContainUnderscoresSuppressor, IdentifiersShouldNotContainUnderscoresAnalyzer>().SuppressionAsync(code, diagnostics, ReferenceAssemblies.NetStandard.NetStandard20, metadataReference, new AssemblyName(assemblyName));
}
