using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class F02001ImplicitRecordClassDeclaration : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.F02001,
		"Implicit record class declaration",
		"Record class '{0}' is declared implicitly",
		DiagnosticCategories.CleanCode,
		DiagnosticSeverity.Warning,
		true,
		"Record classes should be declared explicitly.",
		DiagnosticHelpLinkUris.F02001
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.RecordDeclaration);
	}

	private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
	{
		Debug.Assert(!context.Node.IsKind(SyntaxKind.RecordStructDeclaration));

		if (!HasRecordStructsLanguageFeature(context.Compilation))
		{
			return;
		}

		var declaration = (RecordDeclarationSyntax)context.Node;

		if (declaration.ClassOrStructKeyword.IsKind(SyntaxKind.None))
		{
			if (context.ContainingSymbol is INamedTypeSymbol)
			{
				var location = declaration.Identifier.GetLocation();
				var arg0 = declaration.Identifier.ValueText;

				var diagnostic = Diagnostic.Create(Rule, location, arg0);
				context.ReportDiagnostic(diagnostic);
			}
			else
			{
				Debug.Assert(context.ContainingSymbol is IMethodSymbol,
					$"Unexpected {nameof(context.ContainingSymbol)} '{context.ContainingSymbol.GetType()}'.");
			}
		}
		else
		{
			Debug.Assert(declaration.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
				|| declaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword),
				$"Unexpected {nameof(declaration.ClassOrStructKeyword)} '{declaration.ClassOrStructKeyword.Kind()}'.");
		}
	}

	private static bool HasRecordStructsLanguageFeature(Compilation compilation)
	{
		Debug.Assert(compilation.Language.Equals(LanguageNames.CSharp, StringComparison.Ordinal),
			$"Unexpected {nameof(compilation.Language)} '{compilation.Language}'.");

		var cSharp = (CSharpCompilation)compilation;
		return cSharp.LanguageVersion >= LanguageVersion.CSharp10;
	}
}
