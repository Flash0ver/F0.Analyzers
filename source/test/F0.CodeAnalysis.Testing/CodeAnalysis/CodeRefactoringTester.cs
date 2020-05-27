using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace F0.Testing.CodeAnalysis
{
	internal class CodeRefactoringTester<TCodeRefactoring> : CSharpCodeRefactoringTest<TCodeRefactoring, XUnitVerifier>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		protected override ParseOptions CreateParseOptions()
		{
			var options = base.CreateParseOptions() as CSharpParseOptions;

			return options.WithLanguageVersion(LanguageVersion.CSharp6);
		}
	}
}
