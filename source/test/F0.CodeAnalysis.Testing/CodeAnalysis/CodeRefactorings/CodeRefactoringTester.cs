using Microsoft.CodeAnalysis.CodeRefactorings;

namespace F0.Testing.CodeAnalysis.CodeRefactorings;

internal class CodeRefactoringTester<TCodeRefactoring> : CSharpCodeRefactoringTest<TCodeRefactoring, XUnitVerifier>
	where TCodeRefactoring : CodeRefactoringProvider, new()
{
	internal LanguageVersion? LanguageVersion { get; set; }

	protected override ParseOptions CreateParseOptions()
	{
		var options = (CSharpParseOptions)base.CreateParseOptions();

		return LanguageVersion.HasValue
			? options.WithLanguageVersion(LanguageVersion.Value)
			: options;
	}
}
