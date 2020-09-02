using F0.Benchmarking.CodeAnalysis.CodeFixes;
using F0.Benchmarking.CodeAnalysis.CodeRefactorings;
using F0.Benchmarking.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Benchmarking.CodeAnalysis
{
	public static class Measure
	{
		public static DiagnosticAnalyzerBenchmark<TDiagnosticAnalyzer> DiagnosticAnalyzer<TDiagnosticAnalyzer>()
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			=> new DiagnosticAnalyzerBenchmark<TDiagnosticAnalyzer>();

		public static CodeFixBenchmark<TDiagnosticAnalyzer, TCodeFix> CodeFix<TDiagnosticAnalyzer, TCodeFix>()
			where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
			where TCodeFix : CodeFixProvider, new()
			=> new CodeFixBenchmark<TDiagnosticAnalyzer, TCodeFix>();

		public static CodeRefactoringBenchmark<TCodeRefactoring> CodeRefactoring<TCodeRefactoring>()
			where TCodeRefactoring : CodeRefactoringProvider, new()
			=> new CodeRefactoringBenchmark<TCodeRefactoring>();
	}
}
