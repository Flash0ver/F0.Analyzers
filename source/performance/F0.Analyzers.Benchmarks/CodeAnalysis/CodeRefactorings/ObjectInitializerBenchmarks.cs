using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using F0.Benchmarking.CodeAnalysis;
using F0.Benchmarking.CodeAnalysis.CodeRefactorings;
using F0.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Benchmarks.CodeAnalysis.CodeRefactorings
{
	public class ObjectInitializerBenchmarks
	{
		private readonly CodeRefactoringBenchmark<ObjectInitializer> benchmark;

		public ObjectInitializerBenchmarks()
		{
			benchmark = Measure.CodeRefactoring<ObjectInitializer>();
		}

		[GlobalSetup]
		public void Setup()
		{
			var code =
				@"using System;

				class Model
				{
					public string Text;
					public readonly string Immutable;
					public static string Singleton;

					public Model(string immutable)
					{
						Immutable = immutable;
					}

					public string Local { get; set; }
					public string Parameter { get; set; }
					public string Field { get; set; }
					public string Property { get; set; }
				}

				class C
				{
					string field = ""42"";
					string Property { get; set; }

					void Test(string parameter, int number)
					{
						string local = ""42"";

						var model = new Model(""F0"");
					}

					string Method() => String.Empty;
				}";

			benchmark.Initialize(code, LanguageVersion.Latest, 29, 37, 29, 54, true);
		}

		[Benchmark]
		public Task CreateObjectInitializer()
			=> benchmark.InvokeAsync();

		[GlobalCleanup]
		public void Cleanup()
		{
			var code =
				@"using System;

				class Model
				{
					public string Text;
					public readonly string Immutable;
					public static string Singleton;

					public Model(string immutable)
					{
						Immutable = immutable;
					}

					public string Local { get; set; }
					public string Parameter { get; set; }
					public string Field { get; set; }
					public string Property { get; set; }
				}

				class C
				{
					string field = ""42"";
					string Property { get; set; }

					void Test(string parameter, int number)
					{
						string local = ""42"";

						var model = new Model(""F0"")
						{
							Text = default,
							Local = local,
							Parameter = parameter,
							Field = field,
							Property = Property
						};
					}

					string Method() => String.Empty;
				}";

			benchmark.Inspect(code);
		}
	}
}
