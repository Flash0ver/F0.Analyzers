using F0.Testing.CodeAnalysis.CodeFixes;
using F0.Testing.CodeAnalysis.CodeRefactorings;
using F0.Testing.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis.Suppressors;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Testing.CodeAnalysis;

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

	public static DiagnosticSuppressorVerifier<TDiagnosticSuppressor> DiagnosticSuppressor<TDiagnosticSuppressor>()
		where TDiagnosticSuppressor : DiagnosticSuppressor, new()
		=> new DiagnosticSuppressorVerifier<TDiagnosticSuppressor>();

	public static DiagnosticSuppressorVerifier<TDiagnosticSuppressor, TDiagnosticAnalyzer> DiagnosticSuppressor<TDiagnosticSuppressor, TDiagnosticAnalyzer>()
		where TDiagnosticSuppressor : DiagnosticSuppressor, new()
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		=> new DiagnosticSuppressorVerifier<TDiagnosticSuppressor, TDiagnosticAnalyzer>();

	public static DiagnosticResult Diagnostic<TDiagnosticAnalyzer>(string diagnosticId)
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		=> AnalyzerVerifier<TDiagnosticAnalyzer, CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>, XUnitVerifier>.Diagnostic(diagnosticId);

	public static DiagnosticResult Diagnostic<TDiagnosticAnalyzer>(DiagnosticDescriptor descriptor)
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		=> AnalyzerVerifier<TDiagnosticAnalyzer, CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>, XUnitVerifier>.Diagnostic(descriptor);

	public static DiagnosticResult Diagnostic<TDiagnosticAnalyzer, TCodeFix>(string diagnosticId)
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
		=> CodeFixVerifier<TDiagnosticAnalyzer, TCodeFix, CSharpCodeFixTest<TDiagnosticAnalyzer, TCodeFix, XUnitVerifier>, XUnitVerifier>.Diagnostic(diagnosticId);
}
