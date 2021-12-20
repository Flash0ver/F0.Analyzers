using F0.Testing.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Testing.CodeAnalysis.Diagnostics
{
	public class DiagnosticAnalyzerVerifier<TDiagnosticAnalyzer>
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
	{
		internal DiagnosticAnalyzerVerifier()
		{
		}

		public void Type()
		{
			var type = typeof(TDiagnosticAnalyzer);

			type.VerifyAccessibility();
			type.VerifyNonInheritable();
			type.VerifyDiagnosticAnalyzerAttribute();
		}

		public Task NoOpAsync(string code)
		{
			var tester = CreateTester(code);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task NoOpAsync(string code, ReferenceAssemblies referenceAssemblies, LanguageVersion languageVersion)
		{
			var tester = CreateTester(code, languageVersion, referenceAssemblies);

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

		public Task DiagnosticAsync(string code, IEnumerable<DiagnosticResult> diagnostics, string[][] additionalProjects, ReferenceAssemblies referenceAssemblies, LanguageVersion languageVersion)
		{
			var tester = CreateTester(code, languageVersion, referenceAssemblies, additionalProjects);

			tester.ExpectedDiagnostics.AddRange(diagnostics);

			return tester.RunAsync(CancellationToken.None);
		}

		private static DiagnosticAnalyzerTester<TDiagnosticAnalyzer> CreateTester(string code, LanguageVersion? languageVersion = null, ReferenceAssemblies? referenceAssemblies = null, string[][]? additionalProjects = null)
		{
			var tester = new DiagnosticAnalyzerTester<TDiagnosticAnalyzer>
			{
				TestCode = code.Untabify()
			};

			if (languageVersion.HasValue)
			{
				tester.LanguageVersion = languageVersion;
			}

			if (referenceAssemblies is not null)
			{
				tester.TestState.ReferenceAssemblies = referenceAssemblies;
			}

			if (additionalProjects is not null)
			{
				tester.TestState.AddAdditionalProjects(additionalProjects);
			}

			return tester;
		}
	}
}
