namespace F0.Tests.CodeAnalysis.CodeRefactorings;

public partial class ObjectInitializerTests
{
	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingInstanceFieldExists_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string text = "bowl of petunias";

				void Test()
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
				string text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingFieldsWithValidPrefixedNamesExist_AssignsFieldsToFields()
	{
		var initialCode = """
			using System;

			class C
			{
				string _item1 = "bowl of petunias";
				string s_item2 = "bowl of petunias";
				string t_item3 = "bowl of petunias";

				void Test()
				{
					var model = [|new ValueTuple<string, string, string>()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class C
			{
				string _item1 = "bowl of petunias";
				string s_item2 = "bowl of petunias";
				string t_item3 = "bowl of petunias";

				void Test()
				{
					var model = new ValueTuple<string, string, string>()
					{
						Item1 = _item1,
						Item2 = s_item2,
						Item3 = t_item3
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_FieldWithInvalidPrefixedNameExists_AssignsDefaultToField()
	{
		var initialCode = """
			using System;

			class C
			{
				string m_item1 = "bowl of petunias";

				void Test()
				{
					var model = [|new ValueTuple<string>()|];
				}
			}
			""";

		var expectedCode = """
			using System;

			class C
			{
				string m_item1 = "bowl of petunias";

				void Test()
				{
					var model = new ValueTuple<string>()
					{
						Item1 = default
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingInstanceFieldNameButWrongType_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				int text = 42;

				void Test()
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
				int text = 42;

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingInstanceFieldTypeButWrongName_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string mismatch = "bowl of petunias";

				void Test()
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
				string mismatch = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MultipleMatchingInstanceFields_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string text = "bowl of petunias";
				string Text = "bowl of petunias";

				void Test()
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
				string text = "bowl of petunias";
				string Text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingReadonlyFieldExists_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				readonly string text = "bowl of petunias";

				void Test()
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
				readonly string text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingConstantFieldExists_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				const string text = "bowl of petunias";

				void Test()
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
				const string text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingStaticFieldExists_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				static string text = "bowl of petunias";

				void Test()
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
				static string text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingStaticReadonlyFieldExists_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				static readonly string text = "bowl of petunias";

				void Test()
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
				static readonly string text = "bowl of petunias";

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingInstancePropertyExists_AssignsPropertyToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string Text { get; set; }

				void Test()
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
				string Text { get; set; }

				void Test()
				{
					var model = new Model()
					{
						Text = Text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingInstancePropertyNameButWrongType_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				int text { get; set; }

				void Test()
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
				int text { get; set; }

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingInstancePropertyTypeButWrongName_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string mismatch { get; set; }

				void Test()
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
				string mismatch { get; set; }

				void Test()
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
	public async Task ComputeRefactoringsAsync_MultipleMatchingInstanceProperties_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string text { get; set; }
				string Text { get; set; }

				void Test()
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
				string text { get; set; }
				string Text { get; set; }

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingStaticPropertyExists_AssignsPropertyToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				static string Text { get; set; }

				void Test()
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
				static string Text { get; set; }

				void Test()
				{
					var model = new Model()
					{
						Text = Text
					};
				}
			}
			""";

		await VerifyAsync(initialCode, expectedCode);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_MatchingPropertyWithoutGetAccessor_AssignsDefaultToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string Text { set { } }

				void Test()
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
				string Text { set { } }

				void Test()
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
	public async Task ComputeRefactoringsAsync_MatchingFieldAndMatchingProperty_AssignsFieldToProperty()
	{
		var initialCode = """
			using System;

			class Model { public string Text { get; set; } }

			class C
			{
				string text = "bowl of petunias";
				string Text { get; set; }

				void Test()
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
				string text = "bowl of petunias";
				string Text { get; set; }

				void Test()
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
