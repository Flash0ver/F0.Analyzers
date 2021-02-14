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

				interface IInterface
				{
					int Number { get; set; }
				}

				abstract class ModelBase : IInterface
				{
					public int Public;
					protected int Protected;
					internal int Internal;
					protected internal int Protected_Internal;
					private int Private;
					private protected int Private_Protected;

					public int Number { get; set; }
				}

				sealed class Model : ModelBase
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

					void Test(string parameter, int unused)
					{
						string local = ""42"";

						var model = new Model(""F0"");
					}

					string Method() => String.Empty;
				}";

			benchmark.Initialize(code, LanguageVersion.Latest, 46, 37, 46, 54, true);
		}

		[Benchmark]
		public Task CreateObjectInitializer()
			=> benchmark.InvokeAsync();

		[GlobalCleanup]
		public void Cleanup()
		{
			var code =
				@"using System;

				interface IInterface
				{
					int Number { get; set; }
				}

				abstract class ModelBase : IInterface
				{
					public int Public;
					protected int Protected;
					internal int Internal;
					protected internal int Protected_Internal;
					private int Private;
					private protected int Private_Protected;

					public int Number { get; set; }
				}

				sealed class Model : ModelBase
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

					void Test(string parameter, int unused)
					{
						string local = ""42"";

						var model = new Model(""F0"")
						{
							Public = default,
							Internal = default,
							Protected_Internal = default,
							Number = default,
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
