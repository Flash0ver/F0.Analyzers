using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace F0.Testing.CodeAnalysis.CodeRefactorings
{
	public class CodeRefactoringVerifier<TRefactoringProvider>
		where TRefactoringProvider : CodeRefactoringProvider, new()
	{
		public Task NoOpAsync(string code)
		{
			var tester = CreateTester(code);

			return tester.RunAsync();
		}

		public Task NoOpAsync(string code, LanguageVersion languageVersion)
		{
			var tester = CreateTester(code, null, languageVersion);

			return tester.RunAsync();
		}

		public Task CodeActionAsync(string initialCode, string expectedCode)
		{
			var tester = CreateTester(initialCode, expectedCode);

			return tester.RunAsync();
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, LanguageVersion languageVersion)
		{
			var tester = CreateTester(initialCode, expectedCode, languageVersion);

			return tester.RunAsync();
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, LanguageVersion languageVersion, IEnumerable<Assembly> assemblies)
		{
			var tester = CreateTester(initialCode, expectedCode, languageVersion);

			foreach (var assembly in assemblies)
			{
				tester.TestState.AdditionalReferences.Add(assembly);
			}

			return tester.RunAsync();
		}

		private static CodeRefactoringTester<TRefactoringProvider> CreateTester(string initialCode, string expectedCode = null, LanguageVersion? languageVersion = null)
		{
			var formattedInitialCode = Untabify(initialCode);

			var tester = new CodeRefactoringTester<TRefactoringProvider>
			{
				TestCode = formattedInitialCode,
				FixedCode = expectedCode is null ? formattedInitialCode : Untabify(expectedCode)
			};

			if (languageVersion.HasValue)
			{
				tester.LanguageVersion = languageVersion;
			}

			return tester;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "We prefer the CLR type over the language alias for Constructors.")]
		private static string Untabify(string code)
		{
			if (code.Contains('\t'))
			{
				code = code.Replace("\t", new string(' ', FormattingOptions.IndentationSize.DefaultValue));
			}

			return code;
		}
	}
}
