using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.Diagnostics;
using F0.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Benchmarks.CodeAnalysis.Diagnostics
{
	public class F00001GoToStatementConsideredHarmfulBenchmarks
	{
		private readonly DiagnosticAnalyzerBenchmark<F00001GoToStatementConsideredHarmful> benchmark;

		public F00001GoToStatementConsideredHarmfulBenchmarks()
		{
			benchmark = Measure.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>();
		}

		[GlobalSetup]
		public void Setup()
		{
			var code =
@"using System;

class Class
{
	void Method1()
	{
		Console.WriteLine(""GOTO Considered Harmful"");
	}

	void Method2()
	{
	Label:
		Console.WriteLine(""GOTO Considered Harmful"");
		goto Label;
	}

	void Method3(int number)
	{
		switch (number)
		{
			case 1:
				Console.WriteLine(""goto case 2"");
				goto case 2;
			case 2:
				Console.WriteLine(""goto case 1"");
				goto case 1;
			default:
				Console.WriteLine(""break"");
				break;
		}
	}

	void Method4(int number)
	{
		switch (number)
		{
			case 0:
				Console.WriteLine(""goto default"");
				goto default;
			default:
				Console.WriteLine(""break"");
				break;
		}
	}
}";

			benchmark.Initialize(code, LanguageVersion.Latest);
		}

		[Benchmark]
		public Task GoToStatementConsideredHarmful()
			=> benchmark.InvokeAsync();

		[GlobalCleanup]
		public void Cleanup()
		{
			var diagnostics = benchmark.CreateDiagnostics(
				d => d.WithLocation(14, 9, 14, 20).WithArguments("goto Label;"),
				d => d.WithLocation(23, 17, 23, 29).WithArguments("goto case 2;"),
				d => d.WithLocation(26, 17, 26, 29).WithArguments("goto case 1;"),
				d => d.WithLocation(39, 17, 39, 30).WithArguments("goto default;")
			);

			benchmark.Inspect(diagnostics);
		}
	}
}
