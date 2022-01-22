namespace F0.Tests.CodeAnalysis.CodeRefactorings;

public partial class ObjectInitializerTests
{
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
	public async Task ComputeRefactoringsAsync_ClassWithImmutableMembers_CreatesObjectInitializerWithoutImmutableMembers()
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
	public async Task ComputeRefactoringsAsync_ClassWithStaticMembers_CreatesObjectInitializerWithoutStaticMembers()
	{
		var initialCode =
			@"using System;

				class Model { public string Text { get; set; } public static int Number { get; set; } public static bool Condition; }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

		var expectedCode =
			@"using System;

				class Model { public string Text { get; set; } public static int Number { get; set; } public static bool Condition; }

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

		var externalCode =
			@"namespace F0.Tests.Shared
				{
					public class Model
					{
						public string Text { get; set; }
						public int Number { get; set; }
						public bool Condition { get; set; }
					}
				}";

		await VerifyAsync(initialCode, expectedCode, new string[][] { new[] { externalCode } });
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_PositionalRecordTypeWithPrimaryConstructor_CreatesObjectInitializerWithTheSynthesizedAutoPropertiesFromTheParameterList()
	{
		var initialCode =
			@"using System;

				record Model(string Text, int Number, bool Condition);

				class C
				{
					void Test()
					{
						var model = [|new Model(""F0"", 0x_F0, false)|];
					}
				}";

		var expectedCode =
			@"using System;

				record Model(string Text, int Number, bool Condition);

				class C
				{
					void Test()
					{
						var model = new Model(""F0"", 0x_F0, false)
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

		await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp9, ReferenceAssemblies.Net.Net50);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_RecordTypeWithStandardPropertyAndFieldSyntax_CreatesObjectInitializerWithMembersFromTheRecordBody()
	{
		var initialCode =
			@"using System;

				record Model { public string Text { get; init; } public int Number { get; set; } public bool Condition; }

				class C
				{
					void Test()
					{
						var model = [|new Model()|];
					}
				}";

		var expectedCode =
			@"using System;

				record Model { public string Text { get; init; } public int Number { get; set; } public bool Condition; }

				class C
				{
					void Test()
					{
						var model = new Model()
						{
							Condition = default,
							Text = default,
							Number = default
						};
					}
				}";

		await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp9, ReferenceAssemblies.Net.Net50);
	}

	[Fact]
	public async Task ComputeRefactoringsAsync_RecordTypeWithSynthesizedAndDeclaredProperties_CreatesObjectInitializerWithAllProperties()
	{
		var initialCode =
			@"using System;

				record Model(string Text, int Number) { public int Number { get; init; } public bool Condition { get; init; } }

				class C
				{
					void Test()
					{
						var model = [|new Model(""F0"", 0x_F0)|];
					}
				}";

		var expectedCode =
			@"using System;

				record Model(string Text, int Number) { public int Number { get; init; } public bool Condition { get; init; } }

				class C
				{
					void Test()
					{
						var model = new Model(""F0"", 0x_F0)
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

		await VerifyAsync(initialCode, expectedCode, LanguageVersion.CSharp9, ReferenceAssemblies.Net.Net50);
	}
}
