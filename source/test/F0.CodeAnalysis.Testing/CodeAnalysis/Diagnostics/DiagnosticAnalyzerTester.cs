using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace F0.Testing.CodeAnalysis.Diagnostics
{
	internal class DiagnosticAnalyzerTester<TDiagnosticAnalyzer> : CSharpAnalyzerTest<TDiagnosticAnalyzer, XUnitVerifier>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
	{
	}
}
