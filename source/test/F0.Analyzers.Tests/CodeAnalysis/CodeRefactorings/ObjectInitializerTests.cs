using System.Threading.Tasks;
using F0.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public class ObjectInitializerTests : CodeRefactoringProviderTestFixture
	{
		protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new ObjectInitializer();

		protected override string LanguageName => LanguageNames.CSharp;

		[Fact]
		public async Task ComputeRefactoringsAsync_ToDo_ToDo2()
		{
			var initialCode =
				@"using System;

				class C
				{
					void Test()
					{
						var tuple = [|new ValueTuple<string>()|];
					}
				}";

			var expectedCode =
				@"using System;

				class C
				{
					void Test()
					{
						var tuple = new ValueTuple<string>()
						{
							Item1 = default
						};
					}
				}";

			await TestAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithOneProperty_ToDo4()
		{
			var initialCode =
				@"using System;

				class PropertyBag { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var propertyBag = [|new PropertyBag()|];
					}
				}";

			var expectedCode =
				@"using System;

				class PropertyBag { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var propertyBag = new PropertyBag()
						{
							Text = default
						};
					}
				}";

			await TestAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithMultipleProperties_ToDo4()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; set; } public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var propertyBag = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; set; } public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var propertyBag = new Model()
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

			await TestAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ToDo_ToDo3()
		{
			var initialCode =
				@"using System;
				class C
				{
					void Test()
					{
						string item1 = string.Empty;
						[|var tuple = new ValueTuple<string>();|]
					}
				}";

			var expectedCode =
				@"using System;
				class C
				{
					void Test()
					{
						string item1 = string.Empty;
						var tuple = new ValueTuple<string>
						{
							Item1 = item1
						};
					}
				}";

			await TestAsync(initialCode, expectedCode);
		}
	}

	public class PropertyBag
	{
		public string Text { get; set; }
	}
}
