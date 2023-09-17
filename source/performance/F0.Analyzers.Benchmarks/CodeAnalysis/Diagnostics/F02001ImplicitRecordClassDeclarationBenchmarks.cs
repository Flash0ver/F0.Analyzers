using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.Diagnostics;
using F0.CodeAnalysis.Diagnostics;

namespace F0.Benchmarks.CodeAnalysis.Diagnostics;

public class F02001ImplicitRecordClassDeclarationBenchmarks
{
	private readonly DiagnosticAnalyzerBenchmark<F02001ImplicitRecordClassDeclaration> benchmark;

	public F02001ImplicitRecordClassDeclarationBenchmarks()
	{
		benchmark = Measure.DiagnosticAnalyzer<F02001ImplicitRecordClassDeclaration>();
	}

	[GlobalSetup]
	public void Setup()
	{
		var code = """
			using System;

			record Record(int Number, string Text);
			record class RecordClass(int Number, string Text);
			record struct RecordStruct(int Number, string Text);
			readonly record struct ReadonlyRecordStruct(int Number, string Text);

			[Obsolete]
			sealed record @record<T> : IDisposable where T : notnull
			{
				public T Property { get; init; }

				public void Dispose() => throw new NotImplementedException();
			}
			""";

		benchmark.Initialize(code, LanguageVersion.Latest);
	}

	[Benchmark]
	public Task GoToStatementConsideredHarmful()
		=> benchmark.InvokeAsync();

	[GlobalCleanup]
	public void Cleanup()
	{
		var diagnostics = benchmark.CreateDiagnostics(
			d => d.WithLocation(3, 8, 3, 14).WithArguments("Record"),
			d => d.WithLocation(9, 15, 9, 22).WithArguments("record")
		);

		benchmark.Inspect(diagnostics);
	}
}
