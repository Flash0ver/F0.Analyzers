using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.CodeAnalysis.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class GoToStatementConsideredHarmful : DiagnosticAnalyzer
	{
		private const string DiagnosticId = "F00001";
		private const string Title = "GotoConsideredHarmful";
		private const string MessageFormat = "Don't use goto statements: '{0}'";
		private const string Category = "CodeSmell";
		private const string Description = "GOTO Statement Considered Harmful";
		private const string HelpLinkUri = "https://github.com/Flash0ver/F0.Analyzers";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
			DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(SyntaxNodeAction, GotoStatementSyntaxKinds);
		}

		private static readonly ImmutableArray<SyntaxKind> GotoStatementSyntaxKinds = ImmutableArray.Create(SyntaxKind.GotoStatement,
			SyntaxKind.GotoCaseStatement, SyntaxKind.GotoDefaultStatement);

		private static void SyntaxNodeAction(SyntaxNodeAnalysisContext syntaxNodeContext)
		{
			var diagnostic = Diagnostic.Create(Rule, syntaxNodeContext.Node.GetLocation(),
				syntaxNodeContext.Node.ToString());
			syntaxNodeContext.ReportDiagnostic(diagnostic);
		}
	}
}
