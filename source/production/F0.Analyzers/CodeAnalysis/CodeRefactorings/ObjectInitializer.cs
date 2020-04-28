using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Formatting;

namespace F0.CodeAnalysis.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ObjectInitializer))]
	[Shared]
	public class ObjectInitializer : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var node = root.FindNode(context.Span);

			var objCreationExpr = node as ObjectCreationExpressionSyntax;
			if (objCreationExpr == null)
			{
				return;
			}

			var action = CodeAction.Create("Create Object Initializer", c => CreateObjectInitializer(context.Document, objCreationExpr, c));

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

			var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

			var endOfLineTrivia = SyntaxFactory.EndOfLine(options.GetOption(FormattingOptions.NewLine));

			IEnumerable<ISymbol> mutableFields = typeInfo.Type.GetMembers().OfType<IFieldSymbol>().Where(f => f.DeclaredAccessibility is Accessibility.Public);
			IEnumerable<ISymbol> mutableProperties = typeInfo.Type.GetMembers().OfType<IPropertySymbol>()
				.Where(p => p.SetMethod is IMethodSymbol setMethod && setMethod.DeclaredAccessibility is Accessibility.Public);

			var mutableMembers = mutableFields.Concat(mutableProperties);

			var expressionList = SyntaxFactory.SeparatedList<ExpressionSyntax>();

			foreach (var member in mutableMembers)
			{
				var left = SyntaxFactory.IdentifierName(member.Name);
				var right = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
				var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);

				expressionList = expressionList.Add(expression);
			}

			foreach (var seperator in expressionList.GetSeparators())
			{
				expressionList = expressionList.ReplaceSeparator(seperator, seperator.WithTrailingTrivia(endOfLineTrivia));
			}

			var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressionList);

			SyntaxNode newNode = objCreationExpr.WithInitializer(initializer);

			var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			documentEditor.ReplaceNode(objCreationExpr, newNode);

			return documentEditor.GetChangedDocument();
		}
	}
}
