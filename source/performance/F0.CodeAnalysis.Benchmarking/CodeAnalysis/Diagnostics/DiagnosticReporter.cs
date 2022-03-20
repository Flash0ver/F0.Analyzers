using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Benchmarking.CodeAnalysis.Diagnostics;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1001:Missing diagnostic analyzer attribute.", Justification = "Benchmarking")]
internal sealed class DiagnosticReporter : DiagnosticAnalyzer
{
	private ImmutableArray<DiagnosticDescriptor> supportedDiagnostics;
	private ImmutableArray<LocationFactory> locationFactories;

	public DiagnosticReporter()
	{
	}

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => supportedDiagnostics;

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxTreeAction(SyntaxTreeAction);
	}

	private void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
	{
		var descriptor = supportedDiagnostics.Single();

		foreach (var locationFactory in locationFactories)
		{
			var location = locationFactory.Invoke(context.Tree);
			var diagnostic = Diagnostic.Create(descriptor, location);
			context.ReportDiagnostic(diagnostic);
		}
	}

	public void SetSupportedDiagnostics(DiagnosticSuppressor suppressor, ImmutableArray<LocationFactory> locationFactories)
	{
		supportedDiagnostics = suppressor.SupportedSuppressions
			.Select(static descriptor => descriptor.SuppressedDiagnosticId)
			.Select(static id => CreateDiagnosticDescriptor(id))
			.ToImmutableArray();

		this.locationFactories = locationFactories;
	}

	private static DiagnosticDescriptor CreateDiagnosticDescriptor(string id)
	{
		return new DiagnosticDescriptor(id,
			$"{nameof(DiagnosticDescriptor.Title)} of {id}",
			$"{nameof(DiagnosticDescriptor.MessageFormat)} of {id}",
			$"{nameof(DiagnosticDescriptor.Category)} of {id}",
			DiagnosticSeverity.Warning,
			true,
			$"{nameof(DiagnosticDescriptor.Description)} of {id}",
			$"{nameof(DiagnosticDescriptor.HelpLinkUri)} of {id}");
	}
}
