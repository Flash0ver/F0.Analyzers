namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_NotSupportedSelection_NoOp()
		{
			var code =
				@"using System;

				class Empty { }

				class C
				{
					void Test()$$
					{
						Empty empty = new();
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_CursorBeforeNewStatement_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = $$new();
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_CursorBeforeArgumentList_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new$$();
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_CursorAfterArgumentList_NoAction()
		{
			var code =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new()$$;
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_ArgumentListIsSelected_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new[|()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Implicit_CursorInEmptyArgumentList_CreatesObjectInitializer()
		{
			var initialCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new($$);
					}
				}";

			var expectedCode =
				@"using System;

				class Model { public string Text { get; set; } }

				class C
				{
					void Test()
					{
						Model model = new()
						{
							Text = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}
	}
}
