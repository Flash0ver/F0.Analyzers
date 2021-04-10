using System;
using System.Threading;
using System.Threading.Tasks;
using F0.Testing.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace F0.Testing.CodeAnalysis.CodeRefactorings
{
	public class CodeRefactoringVerifier<TCodeRefactoring>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		private const string LanguageName = LanguageNames.CSharp;

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
			var tester = CreateTester(initialCode, expectedCode, languageVersion, additionalProjects);

			return tester.RunAsync(CancellationToken.None);
		}

		private static CodeRefactoringTester<TCodeRefactoring> CreateTester(string initialCode, string? expectedCode = null, LanguageVersion? languageVersion = null, string[][]? additionalProjects = null)
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

			if (additionalProjects is not null)
			{
				AddAdditionalProjects(tester.TestState, additionalProjects);
			}

			return tester;
		}

		private static void AddAdditionalProjects(SolutionState solution, string[][] additionalProjects)
		{
			for (var projectIndex = 0; projectIndex < additionalProjects.Length; projectIndex++)
			{
				var additionalDocuments = additionalProjects[projectIndex];
				var projectName = Projects.CreateProjectName(projectIndex);

				var project = new ProjectState(projectName, LanguageName, String.Empty, Projects.Extension)
				{
					OutputKind = OutputKind.DynamicallyLinkedLibrary,
					DocumentationMode = DocumentationMode.Diagnose,
				};

				for (var documentIndex = 0; documentIndex < additionalDocuments.Length; documentIndex++)
				{
					var sourceText = additionalDocuments[documentIndex];
					var filename = Documents.CreateDocumentName(projectIndex, documentIndex);

					project.Sources.Add((filename, sourceText));
				}

				solution.AdditionalProjects.Add(projectName, project);
				solution.AdditionalProjectReferences.Add(projectName);
			}
		}
	}
}
