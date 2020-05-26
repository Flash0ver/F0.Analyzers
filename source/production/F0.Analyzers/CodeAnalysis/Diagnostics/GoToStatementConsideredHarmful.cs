using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.CodeAnalysis.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class GoToStatementConsideredHarmful : DiagnosticAnalyzer
	{
		internal const string DiagnosticId = "F00001";
		private const string Title = "GotoConsideredHarmful";
		private const string MessageFormat = "'{0}'";
		private const string Category = "CodeSmell";
		private const string Description = "GOTO Statement Considered Harmful";
		private const string HelpLinkUri = "https://github.com/Flash0ver/F0.Analyzers";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxTreeAction(SyntaxTreeAction);
		}

		private void SyntaxTreeAction(SyntaxTreeAnalysisContext syntaxTreeContext)
		{
			var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);

			foreach (var statement in root.DescendantNodes().OfType<GotoStatementSyntax>())
			{
				var diagnostic = Diagnostic.Create(Rule, statement.GetFirstToken().GetLocation(), statement.ToString());
				syntaxTreeContext.ReportDiagnostic(diagnostic);
			}
		}
	}
}
