using F0.Testing.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis.Diagnostics;
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

		public static CodeRefactoringVerifier<TRefactoringProvider> CodeRefactoring<TRefactoringProvider>()
			where TRefactoringProvider : CodeRefactoringProvider, new()
			=> new CodeRefactoringVerifier<TRefactoringProvider>();

		public static DiagnosticResult Diagnostic<TDiagnosticAnalyzer>(string diagnosticId)
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			=> AnalyzerVerifier<TDiagnosticAnalyzer, CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>, XUnitVerifier>.Diagnostic(diagnosticId);
	}
}
