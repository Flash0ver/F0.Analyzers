using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace F0.Testing.CodeAnalysis.Diagnostics
{
	public class DiagnosticAnalyzerVerifier<TDiagnosticAnalyzer>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
	{
		public Task NoOpAsync(string code)
		{
			var tester = new DiagnosticAnalyzerTester<TDiagnosticAnalyzer>
			{
				TestCode = code
			};

			return tester.RunAsync();
		}

		public Task DiagnosticAsync(string code, DiagnosticResult diagnostic)
		{
			var tester = new DiagnosticAnalyzerTester<TDiagnosticAnalyzer>
			{
				TestCode = code
			};

			tester.ExpectedDiagnostics.Add(diagnostic);

			return tester.RunAsync();
		}

		public Task DiagnosticAsync(string code, IEnumerable<DiagnosticResult> diagnostics)
		{
			var tester = new DiagnosticAnalyzerTester<TDiagnosticAnalyzer>
			{
				TestCode = code
			};

			tester.ExpectedDiagnostics.AddRange(diagnostics);

			return tester.RunAsync();
		}
	}
}
