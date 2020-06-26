﻿using System.Threading.Tasks;
using F0.Tests.Shared;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
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
	}
}
