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

			if (TryGetObjectCreationExpression(node, out var objectCreationExpression))
			{
				if (HasObjectInitializer(objectCreationExpression))
				{
					return;
				}
				else
				{
					var action = CodeAction.Create("Create Object Initializer", c => CreateObjectInitializer(context.Document, objectCreationExpression, c));
					context.RegisterRefactoring(action);
				}
			}
		}

		private static bool TryGetObjectCreationExpression(SyntaxNode node, out ObjectCreationExpressionSyntax objectCreationExpression)
		{
			if (node is ObjectCreationExpressionSyntax nodeExpression)
			{
				objectCreationExpression = nodeExpression;
				return true;
			}

			if (node.Parent is ObjectCreationExpressionSyntax parentExpression)
			{
				objectCreationExpression = parentExpression;
				return true;
			}

			objectCreationExpression = null;
			return false;
		}

		private static bool HasObjectInitializer(ObjectCreationExpressionSyntax objectCreationExpression)
		{
			var childNodes = objectCreationExpression.ChildNodes();
			return childNodes.Any(n => n is InitializerExpressionSyntax);
		}

		private static async Task<Document> CreateObjectInitializer(Document document, ObjectCreationExpressionSyntax objectCreationExpression, CancellationToken cancellationToken)
		{
			var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = compilation.GetSemanticModel(objectCreationExpression.SyntaxTree);

			var typeInfo = semanticModel.GetTypeInfo(objectCreationExpression);

			var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);
			var endOfLineTrivia = SyntaxFactory.EndOfLine(options.GetOption(FormattingOptions.NewLine));

			IEnumerable<ISymbol> mutableFields = typeInfo.Type.GetMembers().OfType<IFieldSymbol>()
				.Where(f => f.DeclaredAccessibility is Accessibility.Public);
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

			if (expressionList.Count is 1)
			{
				var expression = expressionList.Single();
				expressionList = expressionList.Replace(expression, expression.WithTrailingTrivia(endOfLineTrivia));
			}
			else
			{
				foreach (var seperator in expressionList.GetSeparators())
				{
					expressionList = expressionList.ReplaceSeparator(seperator, seperator.WithTrailingTrivia(endOfLineTrivia));
				}
			}

			var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressionList);
			SyntaxNode newNode = objectCreationExpression.WithInitializer(initializer);

			var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			documentEditor.ReplaceNode(objectCreationExpression, newNode);

			return documentEditor.GetChangedDocument();
		}
	}
}
