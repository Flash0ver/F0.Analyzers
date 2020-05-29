using System;
using System.Linq;
using System.Threading.Tasks;
using F0.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis;
using F0.Tests.Shared;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_NotSupportedSelection_NoOp()
		{
			var code =
				@"using System;

				class Empty { }

				class C
				{
					void Test()[||]
					{
						var empty = new Empty();
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ObjectInitializerAlreadyExists_NoOp()
		{
			var code =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = [|new Model()|]
						{
							Text = default
						};
					}
				}";


			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_EmptyClass_EmptyInitializer()
		{
			var initialCode =
				@"using System;

				class Empty { }

				class C
				{
					void Test()
					{
						var empty = [|new Empty()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Empty { }

				class C
				{
					void Test()
					{
						var empty = new Empty() { };
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_GenericStructWithOneField_CreatesObjectInitializerWithOneField()
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

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_GenericStructWithMultipleField_CreatesObjectInitializerWithMultipleField()
		{
			var initialCode =
				@"using System;

				class C
				{
					void Test()
					{
						var tuple = [|new ValueTuple<string, int, bool>()|];
					}
				}";

			var expectedCode =
				@"using System;

				class C
				{
					void Test()
					{
						var tuple = new ValueTuple<string, int, bool>()
						{
							Item1 = default,
							Item2 = default,
							Item3 = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithOneProperty_CreatesObjectInitializerWithOneProperty()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}


		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithMultipleProperties_CreatesObjectInitializerWithMultipleProperties()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; set; } public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; set; } public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}


		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithConstructor_CreatesObjectInitializerAndKeepsParameter()
		{
			var initialCode =
				@"using System;

				class Model { public Model(int number){} public string Text { get; set; }}

				class C
				{
					void Test()
					{
						var model = [|new Model(1)|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public Model(int number){} public string Text { get; set; }}

				class C
				{
					void Test()
					{
						var model = new Model(1)
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassWithImmutableMembers_CreatesObjectInitializerWithoutImmuntableMembers()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; } public readonly bool Condition; }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } public int Number { get; } public readonly bool Condition; }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CSharp2_ObjectInitializerIsNotAvailable()
		{
			var code =
				@"using System;

				class Model { public int Field; private int backingField; public int Property { get { return backingField; } set { backingField = value; } } }

				class C
				{
					void Test()
					{
						Model model = [|new Model()|];
					}
				}";

			await VerifyNoOpAsync(code, LanguageVersion.CSharp2);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CSharp3_ObjectInitializerIsAvailable()
		{
			var initialCode =
				@"using System;

				class Model { public int Field; private int backingField; public int Property { get { return backingField; } set { backingField = value; } } }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public int Field; private int backingField; public int Property { get { return backingField; } set { backingField = value; } } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Field = default(int),
							Property = default(int)
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp3);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CSharp7_DefaultOperator()
		{
			var initialCode =
				@"using System;

				class GlobalType { }

				namespace Namespace.Types
				{
					class NamespacedType { }
				}

				class Model { public GlobalType Global; public Namespace.Types.NamespacedType Namespaced { get; set; } public Tuple<string, Type, System.Data.DbType> Constructed { get; set; } }

				namespace Namespace
				{
					class C
					{
						void Test()
						{
							var model = [|new Model()|];
						}
					}
				}";

			var expectedCode =
				@"using System;

				class GlobalType { }

				namespace Namespace.Types
				{
					class NamespacedType { }
				}

				class Model { public GlobalType Global; public Namespace.Types.NamespacedType Namespaced { get; set; } public Tuple<string, Type, System.Data.DbType> Constructed { get; set; } }

				namespace Namespace
				{
					class C
					{
						void Test()
						{
							var model = new Model()
							{
								Global = default(GlobalType),
								Namespaced = default(Types.NamespacedType),
								Constructed = default(Tuple<string, Type, System.Data.DbType>)
							};
						}
					}
				}";

			await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp7);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CSharp7_1_DefaultLiteral()
		{
			var initialCode =
				@"using System;

				class GlobalType { }

				namespace Namespace.Types
				{
					class NamespacedType { }
				}

				class Model { public GlobalType Global; public Namespace.Types.NamespacedType Namespaced { get; set; } public Tuple<string, Type, System.Data.DbType> Constructed { get; set; } }

				namespace Namespace
				{
					class C
					{
						void Test()
						{
							var model = [|new Model()|];
						}
					}
				}";

			var expectedCode =
				@"using System;

				class GlobalType { }

				namespace Namespace.Types
				{
					class NamespacedType { }
				}

				class Model { public GlobalType Global; public Namespace.Types.NamespacedType Namespaced { get; set; } public Tuple<string, Type, System.Data.DbType> Constructed { get; set; } }

				namespace Namespace
				{
					class C
					{
						void Test()
						{
							var model = new Model()
							{
								Global = default,
								Namespaced = default,
								Constructed = default
							};
						}
					}
				}";

			await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp7_1);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CursorBeforeNewStatement_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = [||]new Model();
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CursorBeforeTypeName_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new [||]Model();
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CursorAfterArgumentList_NoAction()
		{
			var code =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()[||];
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ArgumentListIsSelected_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model[|()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_CursorInEmptyArgumentList_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model([||]);
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ExternalClassWithMultipleProperties_CreatesObjectInitializerWithMultipleProperties()
		{
			var initialCode =
				@"using System;
				using F0.Tests.Shared;

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

			var expectedCode =
				@"using System;
				using F0.Tests.Shared;

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode, typeof(Model));
		}

		private static Task VerifyAsync(string initialCode, string expectedCode, params Type[] types)
		{
			var assemblies = types.Select(t => t.Assembly);
			return Verify.CodeRefactoring<ObjectInitializer>().CodeActionAsync(initialCode, expectedCode, LanguageVersion.Latest, assemblies);
		}

		private static Task VerifyAsync(string initialCode, string expectedCode, LanguageVersion languageVersion)
			=> Verify.CodeRefactoring<ObjectInitializer>().CodeActionAsync(initialCode, expectedCode, languageVersion);

		private static Task VerifyNoOpAsync(string code)
			=> Verify.CodeRefactoring<ObjectInitializer>().NoOpAsync(code, LanguageVersion.Latest);

		private static Task VerifyNoOpAsync(string code, LanguageVersion languageVersion)
			=> Verify.CodeRefactoring<ObjectInitializer>().NoOpAsync(code, languageVersion);


	}
}
