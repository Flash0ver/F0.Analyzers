using F0.Testing.CodeAnalysis.CodeFixes;
using F0.Testing.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace F0.Testing.CodeAnalysis
{
	public static class Verify
	{
		public static DiagnosticAnalyzerVerifier<TDiagnosticAnalyzer> DiagnosticAnalyzer<TDiagnosticAnalyzer>()
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			=> new DiagnosticAnalyzerVerifier<TDiagnosticAnalyzer>();

		public static CodeFixVerifier<TCodeFix> CodeFix<TCodeFix>()
			where TCodeFix : CodeFixProvider, new()
			=> new CodeFixVerifier<TCodeFix>();

		public static CodeFixVerifier<TDiagnosticAnalyzer, TCodeFix> CodeFix<TDiagnosticAnalyzer, TCodeFix>()
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			where TCodeFix : CodeFixProvider, new()
			=> new CodeFixVerifier<TDiagnosticAnalyzer, TCodeFix>();

		public static CodeRefactoringVerifier<TCodeRefactoring> CodeRefactoring<TCodeRefactoring>()
			where TCodeRefactoring : CodeRefactoringProvider, new()
			=> new CodeRefactoringVerifier<TCodeRefactoring>();

		public static DiagnosticResult Diagnostic<TDiagnosticAnalyzer>(string diagnosticId)
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			=> AnalyzerVerifier<TDiagnosticAnalyzer, CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>, XUnitVerifier>.Diagnostic(diagnosticId);
	}
}
