﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F0.Testing.Extensions;
using F0.Testing.Shared;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace F0.Testing.CodeAnalysis.Diagnostics
{
	public class DiagnosticAnalyzerVerifier<TDiagnosticAnalyzer>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
	{
		public void Type()
		{
			var type = typeof(TDiagnosticAnalyzer);

			Check.Accessibility(type);
			Check.NonInheritable(type);
			Check.DiagnosticAnalyzerAttribute(type);
		}

		public Task NoOpAsync(string code)
		{
			var tester = CreateTester(code);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task DiagnosticAsync(string code, DiagnosticResult diagnostic)
		{
			var tester = CreateTester(code);

			tester.ExpectedDiagnostics.Add(diagnostic);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task DiagnosticAsync(string code, IEnumerable<DiagnosticResult> diagnostics)
		{
			var tester = CreateTester(code);

			tester.ExpectedDiagnostics.AddRange(diagnostics);

			return tester.RunAsync(CancellationToken.None);
		}

		private static DiagnosticAnalyzerTester<TDiagnosticAnalyzer> CreateTester(string code)
		{
			var tester = new DiagnosticAnalyzerTester<TDiagnosticAnalyzer>
			{
				TestCode = code.Untabify()
			};

			return tester;
		}
	}
}