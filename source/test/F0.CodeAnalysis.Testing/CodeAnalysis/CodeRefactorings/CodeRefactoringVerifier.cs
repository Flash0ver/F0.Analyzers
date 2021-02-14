using System.Threading;
using System.Threading.Tasks;
using F0.Testing.Extensions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Testing.CodeAnalysis.CodeRefactorings
{
	public class CodeRefactoringVerifier<TCodeRefactoring>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		internal CodeRefactoringVerifier()
		{
		}

		public void Type()
		{
			var type = typeof(TCodeRefactoring);

			type.VerifyAccessibility();
			type.VerifyNonInheritable();
			type.VerifyExportCodeRefactoringProviderAttribute();
			type.VerifySharedAttribute();
		}

		public Task NoOpAsync(string code)
		{
			var tester = CreateTester(code);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task NoOpAsync(string code, LanguageVersion languageVersion)
		{
			var tester = CreateTester(code, default, languageVersion);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode)
		{
			var tester = CreateTester(initialCode, expectedCode);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, LanguageVersion languageVersion)
		{
			var tester = CreateTester(initialCode, expectedCode, languageVersion);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, string[][] additionalProjects, LanguageVersion languageVersion)
		{
			var assertions = CreateAssertions(initialCode, expectedCode, additionalProjects, languageVersion);

			return assertions.RunAsync(CancellationToken.None);
		}

		private static CodeRefactoringTester<TCodeRefactoring> CreateTester(string initialCode, string? expectedCode = null, LanguageVersion? languageVersion = null)
		{
			var normalizedInitialCode = initialCode.Untabify();

			var tester = new CodeRefactoringTester<TCodeRefactoring>
			{
				TestCode = normalizedInitialCode,
				FixedCode = expectedCode is null ? normalizedInitialCode : expectedCode.Untabify()
			};

			if (languageVersion.HasValue)
			{
				tester.LanguageVersion = languageVersion;
			}

			return tester;
		}

		private static CodeRefactoringAssertions<TCodeRefactoring> CreateAssertions(string initialCode, string expectedCode, string[][] additionalProjects, LanguageVersion? languageVersion = null)
		{
			var testCode = initialCode.Untabify();
			var fixedCode = expectedCode.Untabify();
			var assertions = new CodeRefactoringAssertions<TCodeRefactoring>(testCode, fixedCode)
			{
				LanguageVersion = languageVersion
			};

			for (var projectIndex = 0; projectIndex < additionalProjects.Length; projectIndex++)
			{
				for (var documentIndex = 0; documentIndex < additionalProjects[projectIndex].Length; documentIndex++)
				{
					additionalProjects[projectIndex][documentIndex] = additionalProjects[projectIndex][documentIndex].Untabify();
				}
			}

			assertions.AdditionalProjects.AddRange(additionalProjects);

			return assertions;
		}
	}
}
