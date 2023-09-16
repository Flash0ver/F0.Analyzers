using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Testing.CodeAnalysis.Suppressors;

internal sealed class DiagnosticSuppressorTester<TDiagnosticSuppressor, TDiagnosticAnalyzer> : CSharpAnalyzerTest<TDiagnosticSuppressor, XUnitVerifier>
	where TDiagnosticSuppressor : DiagnosticSuppressor, new()
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
{
	private readonly TDiagnosticAnalyzer analyzer = new();

	public DiagnosticSuppressorTester(IEnumerable<Type> metadataReferences)
	{
		TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck;

		var diagnosticOptions = analyzer.SupportedDiagnostics
			.ToImmutableDictionary(static descriptor => descriptor.Id, static descriptor => descriptor.DefaultSeverity.ToReportDiagnostic());

		SolutionTransforms.Add((solution, projectId) =>
		{
			foreach (var metadataReference in metadataReferences)
			{
				var assemblyLocation = metadataReference.Assembly.Location;

				solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(assemblyLocation));
			}

			var project = solution.GetProject(projectId);

			var compilationOptions = (CSharpCompilationOptions)project.CompilationOptions;

			compilationOptions = compilationOptions
				.WithGeneralDiagnosticOption(ReportDiagnostic.Warn)
				.WithSpecificDiagnosticOptions(diagnosticOptions)
				.WithNullableContextOptions(NullableContextOptions.Enable);

			solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

			return solution;
		});

		DiagnosticVerifier = (diagnostic, result, verifier) =>
		{
			var expected = result.IsSuppressed.GetValueOrDefault();

			verifier.Equal(expected, diagnostic.IsSuppressed, $"{nameof(Diagnostic)} {result} is expected to be{(expected ? "" : " not")} suppressed.");
		};
	}

	protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
	{
		var analyzers = new[] { analyzer };

		return base.GetDiagnosticAnalyzers().Concat(analyzers);
	}
}
