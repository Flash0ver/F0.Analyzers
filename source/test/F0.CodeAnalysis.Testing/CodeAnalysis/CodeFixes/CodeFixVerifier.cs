using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F0.Testing.Extensions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace F0.Testing.CodeAnalysis.CodeFixes
{
	public class CodeFixVerifier<TDiagnosticAnalyzer, TCodeFix>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		public Task NoOpAsync(string code, DiagnosticResult diagnostic)
		{
			var tester = CreateTester(code);

			tester.ExpectedDiagnostics.Add(diagnostic);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task NoOpAsync(string code, IEnumerable<DiagnosticResult> diagnostics)
		{
			var tester = CreateTester(code);

			tester.ExpectedDiagnostics.AddRange(diagnostics);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, DiagnosticResult diagnostic, string expectedCode)
		{
			var tester = CreateTester(initialCode, expectedCode);

			tester.ExpectedDiagnostics.Add(diagnostic);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, IEnumerable<DiagnosticResult> diagnostics, string expectedCode)
		{
			var tester = CreateTester(initialCode, expectedCode);

			tester.ExpectedDiagnostics.AddRange(diagnostics);

			return tester.RunAsync(CancellationToken.None);
		}

		private static CodeFixTester<TDiagnosticAnalyzer, TCodeFix> CreateTester(string initialCode, string? expectedCode = null)
		{
			var normalizedInitialCode = initialCode.Untabify();

			var tester = new CodeFixTester<TDiagnosticAnalyzer, TCodeFix>
			{
				TestCode = normalizedInitialCode,
				FixedCode = expectedCode is null ? normalizedInitialCode : expectedCode.Untabify()
			};

			return tester;
		}
	}
}
