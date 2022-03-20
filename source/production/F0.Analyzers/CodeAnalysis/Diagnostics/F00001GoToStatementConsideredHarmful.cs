using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class F00001GoToStatementConsideredHarmful : DiagnosticAnalyzer
{
	[SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/5828")]
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		DiagnosticIds.F00001,
		"GotoConsideredHarmful",
		"Don't use goto statements: '{0}'",
		DiagnosticCategories.CodeSmell,
		DiagnosticSeverity.Warning,
		true,
		"GOTO Statement Considered Harmful.",
		DiagnosticHelpLinkUris.F00001
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(SyntaxNodeAction, GotoStatementSyntaxKinds);
	}

	private static readonly ImmutableArray<SyntaxKind> GotoStatementSyntaxKinds = ImmutableArray.Create(
		SyntaxKind.GotoStatement,
		SyntaxKind.GotoCaseStatement,
		SyntaxKind.GotoDefaultStatement
	);

	private static void SyntaxNodeAction(SyntaxNodeAnalysisContext syntaxNodeContext)
	{
		var diagnostic = Diagnostic.Create(Rule, syntaxNodeContext.Node.GetLocation(),
			syntaxNodeContext.Node.ToString());
		syntaxNodeContext.ReportDiagnostic(diagnostic);
	}
}
