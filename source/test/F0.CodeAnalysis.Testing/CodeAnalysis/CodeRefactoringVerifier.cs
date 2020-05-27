using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;

namespace F0.Testing.CodeAnalysis
{
	public class CodeRefactoringVerifier<TRefactoringProvider>
		where TRefactoringProvider : CodeRefactoringProvider, new()
	{
		public Task NoOpAsync(string code)
		{
			var normalizedCode = Untabify(code);
			var tester = new CodeRefactoringTester<TRefactoringProvider>
			{
				TestCode = normalizedCode,
				FixedCode = normalizedCode
			};

			return tester.RunAsync();
		}

		public Task CodeActionAsync(string initialCode, string expectedCode)
		{
			var tester = new CodeRefactoringTester<TRefactoringProvider>
			{
				TestCode = Untabify(initialCode),
				FixedCode = Untabify(expectedCode)
			};

			return tester.RunAsync();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "We prefer the CLR type over the language alias for Constructors.")]
		private static string Untabify(string code)
		{
			if (code.Contains('\t'))
			{
				code = code.Replace("\t", new String(' ', FormattingOptions.IndentationSize.DefaultValue));
			}

			return code;
		}
	}
}
