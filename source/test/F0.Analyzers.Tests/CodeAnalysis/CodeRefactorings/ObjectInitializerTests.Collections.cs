using System.Threading.Tasks;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_Array_NoOp()
		{
			var code =
				@"using System;

				class C
				{
					void Test()
					{
						var array = [|new int[42]|];
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ArrayList_NoOp()
		{
			var code =
				@"using System;
				using System.Collections;

				class C
				{
					void Test()
					{
						var list = [|new ArrayList()|];
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Collection_NoOp()
		{
			var code =
				@"using System;
				using System.Collections.Generic;

				class C
				{
					void Test()
					{
						var collection = [|new List<int>()|];
					}
				}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_AssociativeCollection_NoOp()
		{
			var code =
				@"using System;
				using System.Collections.Generic;

				class C
				{
					void Test()
					{
						var map = [|new Dictionary<int, string>()|];
					}
				}";

			await VerifyNoOpAsync(code);
		}
	}
}
