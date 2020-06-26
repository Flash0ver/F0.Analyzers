using System.Threading.Tasks;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_NotSupportedSelection_NoOp()
		{
			var code =
				@"using System;

				class Empty { }

				class C
				{
					void Test()$$
					{
						var empty = new Empty();
					}
				}";

			await VerifyNoOpAsync(code);
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
						var model = $$new Model();
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
						var model = new $$Model();
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
						var model = new Model()$$;
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
						var model = new Model($$);
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
	}
}
