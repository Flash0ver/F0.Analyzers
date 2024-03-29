namespace F0.Tests.CodeAnalysis.CodeRefactorings;

public partial class ObjectInitializerTests
{
	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingLocalVariableExists_AssignsVariableToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = "bowl of petunias";
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = "bowl of petunias";
					var model = new Model()
					{
						Text = text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingVariableNameButWrongType_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = 42;
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = 42;
					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingVariableTypeButWrongName_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var mismatch = "bowl of petunias";
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var mismatch = "bowl of petunias";
					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MultipleMatchingVariables_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = "bowl of petunias";
					var Text = "bowl of petunias";

					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					var text = "bowl of petunias";
					var Text = "bowl of petunias";

					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingLocalConstantExists_AssignsConstantToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					const string text = "bowl of petunias";
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test()
				{
					const string text = "bowl of petunias";
					var model = new Model()
					{
						Text = text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingParameterExists_AssignsParameterToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string text)
				{
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string text)
				{
					var model = new Model()
					{
						Text = text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingParameterNameButWrongType_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(char text)
				{
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(char text)
				{
					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingParameterTypeButWrongName_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string mismatch)
				{
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string mismatch)
				{
					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MultipleMatchingParameters_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string text, string Text)
				{
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string text, string Text)
				{
					var model = new Model()
					{
						Text = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingVariableAndMatchingParameter_AssignsVariableToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string Text)
				{
					var text = "bowl of petunias";
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				void Test(string Text)
				{
					var text = "bowl of petunias";
					var model = new Model()
					{
						Text = text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingParameterAndMatchingField_AssignsParameterToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string Text = "bowl of petunias";

				void Test(string text)
				{
					var model = [|new Model()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string Text = "bowl of petunias";

				void Test(string text)
				{
					var model = new Model()
					{
						Text = text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}
}
