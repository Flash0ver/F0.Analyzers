using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.Diagnostics;
using F0.CodeAnalysis;
using F0.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Benchmarks.CodeAnalysis.Diagnostics
{
	public class F0100xPreferPatternMatchingNullCheckOverComparisonWithNullBenchmarks
	{
		private readonly DiagnosticAnalyzerBenchmark<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull> benchmark;

		public F0100xPreferPatternMatchingNullCheckOverComparisonWithNullBenchmarks()
		{
			benchmark = Measure.DiagnosticAnalyzer<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>();
		}

		[GlobalSetup]
		public void Setup()
		{
			var code =
@"using System;

record Record();

class Class
{
	void Method(Record instance)
	{
		_ = instance is null;
		_ = instance is not null;

		_ = instance == null;
		_ = null != instance;

		_ = (object)instance == null;
		_ = null != (object)instance;
	}
}";

			benchmark.Initialize(code, LanguageVersion.Latest);
		}

		[Benchmark]
		public Task PureNullTests()
			=> benchmark.InvokeAsync();

		[GlobalCleanup]
		public void Cleanup()
		{
			var diagnostics = benchmark.CreateDiagnostics(
				(DiagnosticIds.F01001, d => d.WithLocation(12, 13, 12, 29).WithArguments("is", "overloaded", "==", "operator", "null")),
				(DiagnosticIds.F01001, d => d.WithLocation(13, 13, 13, 29).WithArguments("is not", "overloaded", "!=", "operator", "non-null")),
				(DiagnosticIds.F01002, d => d.WithLocation(15, 13, 15, 37).WithArguments("is", "==", "operator", "null")),
				(DiagnosticIds.F01002, d => d.WithLocation(16, 13, 16, 37).WithArguments("is not", "!=", "operator", "non-null"))
			);

			benchmark.Inspect(diagnostics);
		}
	}
}
