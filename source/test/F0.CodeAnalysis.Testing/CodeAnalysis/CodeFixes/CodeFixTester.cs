using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace F0.Testing.CodeAnalysis.CodeFixes
{
	internal class CodeFixTester<TDiagnosticAnalyzer, TCodeFix> : CSharpCodeFixTest<TDiagnosticAnalyzer, TCodeFix, XUnitVerifier>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
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
}
