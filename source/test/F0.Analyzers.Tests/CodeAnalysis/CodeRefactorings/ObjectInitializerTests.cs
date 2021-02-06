using System.Threading.Tasks;
using F0.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public void ObjectInitializer_CheckType()
			=> Verify.CodeRefactoring<ObjectInitializer>().Type();

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

		private static Task VerifyAsync(string initialCode, string expectedCode)
			=> Verify.CodeRefactoring<ObjectInitializer>().CodeActionAsync(initialCode, expectedCode, LanguageVersion.Latest);

		private static Task VerifyAsync(string initialCode, string expectedCode, LanguageVersion languageVersion)
			=> Verify.CodeRefactoring<ObjectInitializer>().CodeActionAsync(initialCode, expectedCode, languageVersion);

		private static Task VerifyAsync(string initialCode, string expectedCode, string[][] additionalProjects)
			=> Verify.CodeRefactoring<ObjectInitializer>().CodeActionAsync(initialCode, expectedCode, additionalProjects, LanguageVersion.Latest);

		private static Task VerifyNoOpAsync(string code)
			=> Verify.CodeRefactoring<ObjectInitializer>().NoOpAsync(code, LanguageVersion.Latest);

		private static Task VerifyNoOpAsync(string code, LanguageVersion languageVersion)
			=> Verify.CodeRefactoring<ObjectInitializer>().NoOpAsync(code, languageVersion);
	}
}
