using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Benchmarks.CodeAnalysis.CodeFixes
{
	public class UsePatternMatchingNullCheckInsteadOfComparisonWithNullBenchmarks
	{
		private readonly CodeFixBenchmark<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull> benchmark;

		public UsePatternMatchingNullCheckInsteadOfComparisonWithNullBenchmarks()
		{
			benchmark = Measure.CodeFix<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>();
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
		_ = (object)instance != null;
	}
}";

			benchmark.Initialize(code, LanguageVersion.Latest);
		}

		[Benchmark]
		public Task UseConstantNullPattern()
			=> benchmark.InvokeAsync();

		[GlobalCleanup]
		public void Cleanup()
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
		_ = instance is not null;
	}
}";

			benchmark.Inspect(code);
		}
	}
}
