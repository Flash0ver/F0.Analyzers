using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace F0.CodeAnalysis.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DeclareRecordClassExplicitly))]
[Shared]
internal sealed class DeclareRecordClassExplicitly : CodeFixProvider
{
	private const string Title = "Explicitly add class keyword to reference record declaration";

	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.F02001);
	public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		Debug.Assert(context.Document.SupportsSyntaxTree is true, $"Document doesn't support providing data through '{nameof(Document.GetSyntaxRootAsync)}'.");
		Debug.Assert(root is not null);

		var node = root.FindNode(context.Span);
		var declaration = (RecordDeclarationSyntax)node;

		var diagnostic = context.Diagnostics.First();

		var action = CodeAction.Create(Title, cancellationToken => ExplicitlyAddClassKeywordToReferenceRecordAsync(context.Document, root, declaration), diagnostic.Id);
		context.RegisterCodeFix(action, diagnostic);
	}

	private static Task<Document> ExplicitlyAddClassKeywordToReferenceRecordAsync(Document oldDocument, SyntaxNode oldRoot, RecordDeclarationSyntax oldNode)
	{
		Debug.Assert(oldNode.ClassOrStructKeyword.IsKind(SyntaxKind.None));

		var token = SyntaxFactory.Token(SyntaxKind.ClassKeyword);

		var newNode = oldNode.WithClassOrStructKeyword(token);

		var newRoot = oldRoot.ReplaceNode(oldNode, newNode);

		var newDocument = oldDocument.WithSyntaxRoot(newRoot);

		return Task.FromResult(newDocument);
	}
}
