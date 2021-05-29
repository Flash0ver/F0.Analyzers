using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace F0.CodeAnalysis.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePatternMatchingNullCheckInsteadOfComparisonWithNull))]
	[Shared]
	internal sealed class UsePatternMatchingNullCheckInsteadOfComparisonWithNull : CodeFixProvider
	{
		private const string Title = "Use constant 'null' pattern";

		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.F01001, DiagnosticIds.F01002);
		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			Debug.Assert(root is not null, $"Document doesn't support providing data: {{ {nameof(Document.SupportsSyntaxTree)} = {context.Document.SupportsSyntaxTree} }}");
			var node = root.FindNode(context.Span, false, true);

			Debug.Assert(context.Diagnostics.Length is 1);
			var diagnostic = context.Diagnostics[0];
			var action = CodeAction.Create(Title, ct => ReplaceWithPatternMatching(context.Document, node, ct), diagnostic.Id);
			context.RegisterCodeFix(action, diagnostic);
		}

		private static Task<Document> ReplaceWithPatternMatching(Document document, SyntaxNode node, CancellationToken cancellationToken)
		{
			var expression = node switch
			{
				ExpressionSyntax syntax => syntax,
				_ => throw new ArgumentException($"Invalid argument '{node}' of type '{node.GetType()}' was specified.", nameof(node)),
			};

			return ReplaceWithPatternMatching(document, expression, cancellationToken);
		}

		private static async Task<Document> ReplaceWithPatternMatching(Document document, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var newExpression = expression switch
			{
				BinaryExpressionSyntax binary => CreateFromBinaryExpression(binary),
				InvocationExpressionSyntax invocation => CreatePatternExpression(invocation),
				PrefixUnaryExpressionSyntax prefixUnary => CreateNegatedPatternExpression(prefixUnary),
				_ => throw new ArgumentException($"Invalid argument '{expression}' of type '{expression.GetType()}' was specified.", nameof(expression)),
			};

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			Debug.Assert(root is not null, $"Document doesn't support providing data: {{ {nameof(Document.SupportsSyntaxTree)} = {document.SupportsSyntaxTree} }}");
			var newRoot = root.ReplaceNode(expression, newExpression);

			return document.WithSyntaxRoot(newRoot);

			static IsPatternExpressionSyntax CreateFromBinaryExpression(BinaryExpressionSyntax binary)
			{
				var syntax = binary.Right.IsKind(SyntaxKind.NullLiteralExpression)
					? binary.Left
					: binary.Right;

				if (binary.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
				{
					return CreatePatternExpression(syntax);
				}
				else
				{
					Debug.Assert(binary.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken));
					return CreateNegatedPatternExpression(syntax);
				}
			}
		}

		private static IsPatternExpressionSyntax CreatePatternExpression(ExpressionSyntax expression)
		{
			var literal = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
			var pattern = SyntaxFactory.ConstantPattern(literal);

			return expression switch
			{
				CastExpressionSyntax cast => SyntaxFactory.IsPatternExpression(cast.Expression, pattern),
				ParenthesizedExpressionSyntax { Expression: CastExpressionSyntax cast } => SyntaxFactory.IsPatternExpression(cast.Expression, pattern),
				_ => SyntaxFactory.IsPatternExpression(expression, pattern),
			};
		}

		private static IsPatternExpressionSyntax CreateNegatedPatternExpression(ExpressionSyntax expression)
		{
			var literal = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
			var pattern = SyntaxFactory.UnaryPattern(SyntaxFactory.ConstantPattern(literal));

			return expression switch
			{
				CastExpressionSyntax cast => SyntaxFactory.IsPatternExpression(cast.Expression, pattern),
				ParenthesizedExpressionSyntax { Expression: CastExpressionSyntax cast } => SyntaxFactory.IsPatternExpression(cast.Expression, pattern),
				_ => SyntaxFactory.IsPatternExpression(expression, pattern),
			};
		}

		private static IsPatternExpressionSyntax CreatePatternExpression(InvocationExpressionSyntax invocation)
		{
			Debug.Assert(invocation.Expression is MemberAccessExpressionSyntax);

			if (invocation.ArgumentList.Arguments.Count is 2)
			{
				var first = invocation.ArgumentList.Arguments[0].Expression;
				var second = invocation.ArgumentList.Arguments[1].Expression;

				return second.IsKind(SyntaxKind.NullLiteralExpression)
					? CreatePatternExpression(first)
					: CreatePatternExpression(second);
			}
			else
			{
				Debug.Assert(invocation.ArgumentList.Arguments.Count is 1);

				var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
				return CreatePatternExpression(memberAccess.Expression);
			}
		}

		private static IsPatternExpressionSyntax CreateNegatedPatternExpression(PrefixUnaryExpressionSyntax prefixUnary)
		{
			var invocation = (InvocationExpressionSyntax)prefixUnary.Operand;
			Debug.Assert(invocation.Expression is MemberAccessExpressionSyntax);

			if (invocation.ArgumentList.Arguments.Count is 2)
			{
				var first = invocation.ArgumentList.Arguments[0].Expression;
				var second = invocation.ArgumentList.Arguments[1].Expression;

				return second.IsKind(SyntaxKind.NullLiteralExpression)
					? CreateNegatedPatternExpression(first)
					: CreateNegatedPatternExpression(second);
			}
			else
			{
				Debug.Assert(invocation.ArgumentList.Arguments.Count is 1);

				var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
				return CreateNegatedPatternExpression(memberAccess.Expression);
			}
		}
	}
}
