using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.Suppressors;
using F0.CodeAnalysis.Suppressors;

namespace F0.Benchmarks.CodeAnalysis.Suppressors;

public class CA1707IdentifiersShouldNotContainUnderscoresSuppressorBenchmarks
{
	private readonly DiagnosticSuppressorBenchmark<CA1707IdentifiersShouldNotContainUnderscoresSuppressor> benchmark;

	public CA1707IdentifiersShouldNotContainUnderscoresSuppressorBenchmarks()
	{
		benchmark = Measure.DiagnosticSuppressor<CA1707IdentifiersShouldNotContainUnderscoresSuppressor>();
	}

	[GlobalSetup]
	public void Setup()
	{
		var code = """
			using Xunit;

			public class xUnit_Tests
			{
				[Fact]
				public void Given_When_Then()
				{
					Assert.Equal(240, 0x_F0);
				}

				[Theory]
				[InlineData(0x_F0)]
				public void MethodUnderTest_Scenario_ExpectedResult(int value)
				{
					Assert.Equal(240, value);
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
			}
			""";

		var locations = benchmark.CreateLocations(
			l => l.With(3, 14, 3, 25),
			l => l.With(6, 17, 6, 32),
			l => l.With(13, 17, 13, 56)
		);

		benchmark.Initialize(code, "xunit.core", LanguageVersion.Latest, locations);
	}

	[Benchmark]
	public Task IdentifiersShouldNotContainUnderscoresSuppressor()
		=> benchmark.InvokeAsync();

	[GlobalCleanup]
	public void Cleanup()
	{
		var diagnostics = benchmark.CreateDiagnostics(
			d => d.WithLocation(3, 14, 3, 25).WithIsSuppressed(false),
			d => d.WithLocation(6, 17, 6, 32).WithIsSuppressed(true),
			d => d.WithLocation(13, 17, 13, 56).WithIsSuppressed(true)
		);

		benchmark.Inspect(diagnostics);
	}
}
