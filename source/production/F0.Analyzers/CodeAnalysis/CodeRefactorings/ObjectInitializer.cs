using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using F0.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;

namespace F0.CodeAnalysis.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ObjectInitializer))]
	[Shared]
	internal sealed class ObjectInitializer : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			if (!HasObjectInitializerFeature(context.Document.Project))
			{
				return;
			}

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			if (TryGetObjectCreationExpression(node, out var objectCreationExpression))
			{
				if (!HasObjectInitializer(objectCreationExpression))
				{
					var action = CodeAction.Create("Create Object Initializer", c => CreateObjectInitializer(context.Document, objectCreationExpression, c));
					context.RegisterRefactoring(action);
				}
			}
		}

		private static bool HasObjectInitializerFeature(Project project)
		{
			var parseOptions = (CSharpParseOptions)project.ParseOptions;
			return parseOptions.LanguageVersion >= LanguageVersion.CSharp3;
		}

		private static bool TryGetObjectCreationExpression(SyntaxNode node, [NotNullWhen(true)] out ObjectCreationExpressionSyntax? objectCreationExpression)
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
			var mutableMembers = GetMutableMembers(ref typeInfo);

			var availableSymbols = semanticModel.LookupSymbols(objectCreationExpression.SpanStart);

			var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

			var expressionList = CreateAssignmentExpressions(document, mutableMembers, availableSymbols);
			expressionList = FormatExpressionList(expressionList, options);

			var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressionList);
			SyntaxNode newNode = objectCreationExpression.WithInitializer(initializer);

			var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			documentEditor.ReplaceNode(objectCreationExpression, newNode);

			return documentEditor.GetChangedDocument();
		}

		private static IEnumerable<ISymbol> GetMutableMembers(ref TypeInfo typeInfo)
		{
			var instanceMembers = typeInfo.Type.GetMembers().Where(s => !s.IsStatic);

			IEnumerable<ISymbol> mutableFields = instanceMembers.OfType<IFieldSymbol>()
				.Where(f => f.DeclaredAccessibility is Accessibility.Public && !f.IsReadOnly);
			IEnumerable<ISymbol> mutableProperties = instanceMembers.OfType<IPropertySymbol>()
				.Where(p => p.SetMethod is IMethodSymbol setMethod && setMethod.DeclaredAccessibility is Accessibility.Public);

			var mutableMembers = mutableFields.Concat(mutableProperties);
			return mutableMembers;
		}

		private static SeparatedSyntaxList<ExpressionSyntax> CreateAssignmentExpressions(Document document, IEnumerable<ISymbol> mutableMembers, IEnumerable<ISymbol> symbols)
		{
			var localSymbols = symbols.Where(s => s.Kind is SymbolKind.Local).Cast<ILocalSymbol>().ToArray();
			var parameterSymbols = symbols.Where(s => s.Kind is SymbolKind.Parameter).Cast<IParameterSymbol>().ToArray();
			var fieldSymbols = symbols.Where(s => s.Kind is SymbolKind.Field).Cast<IFieldSymbol>().ToArray();
			var propertySymbols = symbols.Where(s => s.Kind is SymbolKind.Property).Cast<IPropertySymbol>().ToArray();

			var hasDefaultLiteral = HasDefaultLiteralFeature(document.Project);
			var generator = hasDefaultLiteral ? null : SyntaxGenerator.GetGenerator(document);

			var expressionList = SyntaxFactory.SeparatedList<ExpressionSyntax>();

			foreach (var member in mutableMembers)
			{
				var left = SyntaxFactory.IdentifierName(member.Name);

				ExpressionSyntax right;

				var matchingSymbol = GetMatchingSymbol(member, localSymbols, parameterSymbols, fieldSymbols, propertySymbols);

				if (matchingSymbol is { })
				{
					right = SyntaxFactory.IdentifierName(matchingSymbol.Name);
				}
				else if (hasDefaultLiteral)
				{
					right = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
				}
				else
				{
					var type = GetMemberType(member);
					var typeExpression = generator!.TypeExpression(type);

					right = (DefaultExpressionSyntax)generator.DefaultExpression(typeExpression);
				}

				var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
				expressionList = expressionList.Add(expression);
			}

			return expressionList;
		}

		private static bool HasDefaultLiteralFeature(Project project)
		{
			var parseOptions = (CSharpParseOptions)project.ParseOptions;
			return parseOptions.LanguageVersion >= LanguageVersion.CSharp7_1;
		}

		private static ISymbol? GetMatchingSymbol(ISymbol member, IEnumerable<ILocalSymbol> localSymbols, IEnumerable<IParameterSymbol> parameterSymbols, IEnumerable<IFieldSymbol> fieldSymbols, IEnumerable<IPropertySymbol> propertySymbols)
		{
			return GetLocalSymbol(member, localSymbols)
				?? GetParameterSymbol(member, parameterSymbols)
				?? GetFieldSymbol(member, fieldSymbols)
				?? GetPropertySymbol(member, propertySymbols);

			static ISymbol? GetLocalSymbol(ISymbol member, IEnumerable<ILocalSymbol> localSymbols)
			{
				return localSymbols
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type == GetMemberType(member));
			}

			static ISymbol? GetParameterSymbol(ISymbol member, IEnumerable<IParameterSymbol> parameterSymbols)
			{
				return parameterSymbols
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type == GetMemberType(member));
			}

			static ISymbol? GetFieldSymbol(ISymbol member, IEnumerable<IFieldSymbol> fieldSymbols)
			{
				return fieldSymbols
					.SoleOrDefault(s => GetPlainName(s).Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type == GetMemberType(member));
			}

			static ISymbol? GetPropertySymbol(ISymbol member, IEnumerable<IPropertySymbol> propertySymbols)
			{
				return propertySymbols
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type == GetMemberType(member) && s.GetMethod is IMethodSymbol);
			}
		}

		private static INamedTypeSymbol GetMemberType(ISymbol member)
		{
			if (member is IPropertySymbol property)
			{
				return (INamedTypeSymbol)property.Type;
			}

			if (member is IFieldSymbol field)
			{
				return (INamedTypeSymbol)field.Type;
			}

			throw new NotSupportedException();
		}

		private static string GetPlainName(IFieldSymbol fieldSymbol)
		{
			var name = fieldSymbol.Name;

			if (name.StartsWith("_"))
			{
				name = name.Substring(1);
			}
			else if (name.StartsWith("s_") || name.StartsWith("t_"))
			{
				name = name.Substring(2);
			}

			return name;
		}

		private static SeparatedSyntaxList<ExpressionSyntax> FormatExpressionList(SeparatedSyntaxList<ExpressionSyntax> expressionList, DocumentOptionSet options)
		{
			var endOfLineTrivia = SyntaxFactory.EndOfLine(options.GetOption(FormattingOptions.NewLine));

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

			return expressionList;
		}
	}
}
