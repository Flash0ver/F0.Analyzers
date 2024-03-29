using System.Diagnostics;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace F0.Benchmarks;

internal static class Program
{
	private static void Main(string[] args)
	{
		var config = Debugger.IsAttached
			? CreateDebugConfiguration()
			: CreateBenchmarkConfiguration();

		_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

		static IConfig CreateBenchmarkConfiguration()
			=> DefaultConfig.Instance
				.AddJob(Job.InProcess.WithRuntime(ClrRuntime.Net472))
				.AddJob(Job.InProcess.WithRuntime(CoreRuntime.Core70))
				.AddColumn(StatisticColumn.Min, StatisticColumn.Max, StatisticColumn.Median)
				.AddDiagnoser(MemoryDiagnoser.Default)
				.AddExporter(DefaultExporters.AsciiDoc)
				.AddValidator(ExecutionValidator.FailOnError);

		static IConfig CreateDebugConfiguration()
			=> new DebugInProcessConfig();
	}
}
