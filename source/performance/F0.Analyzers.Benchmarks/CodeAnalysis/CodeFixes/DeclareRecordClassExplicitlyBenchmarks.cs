using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.Diagnostics;

namespace F0.Benchmarks.CodeAnalysis.CodeFixes;

public class DeclareRecordClassExplicitlyBenchmarks
{
	private readonly CodeFixBenchmark<F02001ImplicitRecordClassDeclaration, DeclareRecordClassExplicitly> benchmark;

	public DeclareRecordClassExplicitlyBenchmarks()
	{
		benchmark = Measure.CodeFix<F02001ImplicitRecordClassDeclaration, DeclareRecordClassExplicitly>();
	}

	[GlobalSetup]
	public void Setup()
	{
		var code = """
			using System;

			record Record;
			""";

		benchmark.Initialize(code, LanguageVersion.Latest);
	}

	[Benchmark]
	public Task DeclareRecordClassExplicitly()
		=> benchmark.InvokeAsync();

	[GlobalCleanup]
	public void Cleanup()
	{
		var code = """
			using System;

			record class Record;
			""";

		benchmark.Inspect(code);
	}
}
