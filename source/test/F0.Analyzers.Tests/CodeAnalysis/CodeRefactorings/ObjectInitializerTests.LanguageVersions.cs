using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
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
	}
}
