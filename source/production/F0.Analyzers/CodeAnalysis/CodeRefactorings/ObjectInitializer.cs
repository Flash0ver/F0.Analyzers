using System.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace F0.CodeAnalysis.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ObjectInitializer))]
	[Shared]
	public class ObjectInitializer : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			// Find the node at the selection.
			var node = root.FindNode(context.Span);

			// Only offer a refactoring if the selected node is a type declaration node.
			var objCreationExpr = node as ObjectCreationExpressionSyntax;
			if (objCreationExpr == null)
			{
				return;
			}

			var action = CodeAction.Create("Create Object Initializer", c => CreateObjectInitializer(context.Document, objCreationExpr, c));

			// Register this code action.
			context.RegisterRefactoring(action);
		}

		private async Task<Document> CreateObjectInitializer(Document document, ObjectCreationExpressionSyntax objCreationExpr, CancellationToken cancellationToken)
		{
			var compilation = CSharpCompilation.Create("TestCompilation")
				.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
				.AddSyntaxTrees(objCreationExpr.SyntaxTree);

			var syntaxTree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var semanticModel = compilation.GetSemanticModel(syntaxTree);

			var typeSyntax = objCreationExpr.Type;

			var typeInfo = semanticModel.GetTypeInfo(objCreationExpr);

			var fields = typeInfo.Type.GetMembers().OfType<IFieldSymbol>().Where(f => f.DeclaredAccessibility is Accessibility.Public);
			var properties = typeInfo.Type.GetMembers().OfType<IPropertySymbol>()
				.Where(p => p.SetMethod is IMethodSymbol setMethod && setMethod.DeclaredAccessibility is Accessibility.Public);

			var expressionList = SyntaxFactory.SeparatedList<ExpressionSyntax>();

			foreach (var field in fields)
			{
				var left = SyntaxFactory.IdentifierName(field.Name);
				var right = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
				var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
				expressionList = expressionList.Add(expression);
			}

			var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressionList);

			SyntaxNode newNode = objCreationExpr.WithInitializer(initializer);

			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			editor.ReplaceNode(objCreationExpr, newNode);

			return editor.GetChangedDocument();
		}
	}
}
