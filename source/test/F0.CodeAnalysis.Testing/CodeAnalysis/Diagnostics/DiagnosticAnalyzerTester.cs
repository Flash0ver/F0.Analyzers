using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Testing.CodeAnalysis.Diagnostics;

internal sealed class DiagnosticAnalyzerTester<TDiagnosticAnalyzer> : CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
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
