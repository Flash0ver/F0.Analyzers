using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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
		private const string Title = "Create Object Initializer";

		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			if (!HasObjectInitializerFeature(context.Document.Project))
			{
				return;
			}

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			Debug.Assert(root is not null, $"Document doesn't support providing data: {{ {nameof(Document.SupportsSyntaxTree)} = {context.Document.SupportsSyntaxTree} }}");
			var node = root.FindNode(context.Span);

			if (TryGetObjectCreationExpression(node, out var objectCreationExpression) && !HasObjectInitializer(objectCreationExpression))
			{
				var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken).ConfigureAwait(false);
				Debug.Assert(compilation is not null, $"Project doesn't support producing compilations: {{ {nameof(Project.SupportsCompilation)} = {context.Document.Project.SupportsCompilation} }}");
				var semanticModel = compilation.GetSemanticModel(objectCreationExpression.SyntaxTree);

				var typeInfo = semanticModel.GetTypeInfo(objectCreationExpression, context.CancellationToken);
				Debug.Assert(typeInfo.Type is not null, $"Expected {nameof(BaseObjectCreationExpressionSyntax)} to have a type");
				Debug.Assert(typeInfo.Type is not IErrorTypeSymbol, $"Type could not be determined due to an error: {typeInfo.Type}");

				if (!IsCollection(typeInfo.Type))
				{
					var action = CodeAction.Create(Title, ct => CreateObjectInitializer(context.Document, semanticModel, objectCreationExpression, typeInfo, ct));
					context.RegisterRefactoring(action);
				}
			}
		}

		private static bool HasObjectInitializerFeature(Project project)
		{
			Debug.Assert(project.ParseOptions is not null, $"{nameof(project.ParseOptions)} is null unexpectedly");
			var parseOptions = (CSharpParseOptions)project.ParseOptions;
			return parseOptions.LanguageVersion >= LanguageVersion.CSharp3;
		}

		private static bool TryGetObjectCreationExpression(SyntaxNode node, [NotNullWhen(true)] out BaseObjectCreationExpressionSyntax? objectCreationExpression)
		{
			if (node is BaseObjectCreationExpressionSyntax nodeExpression)
			{
				objectCreationExpression = nodeExpression;
				return true;
			}

			if (node.Parent is BaseObjectCreationExpressionSyntax parentExpression)
			{
				objectCreationExpression = parentExpression;
				return true;
			}

			objectCreationExpression = null;
			return false;
		}

		private static bool HasObjectInitializer(BaseObjectCreationExpressionSyntax objectCreationExpression)
		{
			var childNodes = objectCreationExpression.ChildNodes();
			return childNodes.Any(n => n is InitializerExpressionSyntax);
		}

		private static bool IsCollection(ITypeSymbol type)
		{
			var interfaces = type.AllInterfaces;
			return interfaces.Any(i => i.SpecialType is SpecialType.System_Collections_IEnumerable);
		}

		private static async Task<Document> CreateObjectInitializer(Document document, SemanticModel semanticModel, BaseObjectCreationExpressionSyntax objectCreationExpression, TypeInfo typeInfo, CancellationToken cancellationToken)
		{
			var mutableMembers = GetMutableMembers(typeInfo, semanticModel.Compilation);

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

		private static IEnumerable<ISymbol> GetMutableMembers(in TypeInfo typeInfo, Compilation compilation)
		{
			var members = new HashSet<ISymbol>(SymbolNameComparer.Instance);
			var mutableMembers = new List<ISymbol>();

			var type = typeInfo.Type;
			while (IsExplicitBaseType(type))
			{
				var instanceMembers = type.GetMembers().Where(s => !s.IsStatic).ToImmutableArray();
				var areInternalSymbolsAccessible = type.ContainingAssembly.GivesAccessTo(compilation.Assembly);

				var mutableFields = instanceMembers
					.OfType<IFieldSymbol>()
					.Where(members.Add)
					.Where(field => FilterMutableFields(field, areInternalSymbolsAccessible));
				var mutableProperties = instanceMembers
					.OfType<IPropertySymbol>()
					.Where(members.Add)
					.Where(property => FilterMutableProperties(property, areInternalSymbolsAccessible));

				mutableMembers.InsertRange(0, mutableProperties);
				mutableMembers.InsertRange(0, mutableFields);

				type = type.BaseType;
			}

			return mutableMembers;

			static bool IsExplicitBaseType([NotNullWhen(true)] ITypeSymbol? type)
				=> type is not null && type.SpecialType is not SpecialType.System_Object and not SpecialType.System_ValueType;

			static bool FilterMutableFields(IFieldSymbol field, bool areInternalSymbolsAccessible)
			{
				return !field.IsReadOnly
					&& IsAccessible(field, areInternalSymbolsAccessible);
			}

			static bool FilterMutableProperties(IPropertySymbol property, bool areInternalSymbolsAccessible)
			{
				return property.SetMethod is { } setMethod
					&& IsAccessible(setMethod, areInternalSymbolsAccessible);
			}
		}

		private static bool IsAccessible(ISymbol symbol, bool isLocationWithinFriendAssembly)
		{
			var accessibility = symbol.DeclaredAccessibility;
			return accessibility is Accessibility.Public
				|| (isLocationWithinFriendAssembly && (accessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal));
		}

		private static SeparatedSyntaxList<ExpressionSyntax> CreateAssignmentExpressions(Document document, IEnumerable<ISymbol> mutableMembers, ImmutableArray<ISymbol> symbols)
		{
			var localSymbols = symbols.Where(s => s.Kind is SymbolKind.Local).Cast<ILocalSymbol>().ToImmutableArray();
			var parameterSymbols = symbols.Where(s => s.Kind is SymbolKind.Parameter).Cast<IParameterSymbol>().ToImmutableArray();
			var fieldSymbols = symbols.Where(s => s.Kind is SymbolKind.Field).Cast<IFieldSymbol>().ToImmutableArray();
			var propertySymbols = symbols.Where(s => s.Kind is SymbolKind.Property).Cast<IPropertySymbol>().ToImmutableArray();

			var hasDefaultLiteral = HasDefaultLiteralFeature(document.Project);
			var generator = hasDefaultLiteral ? null : SyntaxGenerator.GetGenerator(document);

			var expressionList = SyntaxFactory.SeparatedList<ExpressionSyntax>();

			foreach (var member in mutableMembers)
			{
				var left = SyntaxFactory.IdentifierName(member.Name);

				ExpressionSyntax right;

				var matchingSymbol = GetMatchingSymbol(member, localSymbols, parameterSymbols, fieldSymbols, propertySymbols);

				if (matchingSymbol is not null)
				{
					right = SyntaxFactory.IdentifierName(matchingSymbol.Name);
				}
				else if (hasDefaultLiteral)
				{
					Debug.Assert(generator is null);
					right = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
				}
				else
				{
					Debug.Assert(generator is not null);
					var type = GetMemberType(member);
					var typeExpression = generator.TypeExpression(type);

					right = (DefaultExpressionSyntax)generator.DefaultExpression(typeExpression);
				}

				var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
				expressionList = expressionList.Add(expression);
			}

			return expressionList;
		}

		private static bool HasDefaultLiteralFeature(Project project)
		{
			Debug.Assert(project.ParseOptions is not null, $"{nameof(project.ParseOptions)} is null unexpectedly");
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
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type.Equals(GetMemberType(member), SymbolEqualityComparer.Default));
			}

			static ISymbol? GetParameterSymbol(ISymbol member, IEnumerable<IParameterSymbol> parameterSymbols)
			{
				return parameterSymbols
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type.Equals(GetMemberType(member), SymbolEqualityComparer.Default));
			}

			static ISymbol? GetFieldSymbol(ISymbol member, IEnumerable<IFieldSymbol> fieldSymbols)
			{
				return fieldSymbols
					.SoleOrDefault(s => GetPlainName(s).Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type.Equals(GetMemberType(member), SymbolEqualityComparer.Default));
			}

			static ISymbol? GetPropertySymbol(ISymbol member, IEnumerable<IPropertySymbol> propertySymbols)
			{
				return propertySymbols
					.SoleOrDefault(s => s.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) && s.Type.Equals(GetMemberType(member), SymbolEqualityComparer.Default) && s.GetMethod is not null);
			}
		}

		private static INamedTypeSymbol GetMemberType(ISymbol member)
		{
			return member switch
			{
				IPropertySymbol property => (INamedTypeSymbol)property.Type,
				IFieldSymbol field => (INamedTypeSymbol)field.Type,
				_ => throw new ArgumentException($"Invalid argument '{member}' of type '{member.GetType()}' was specified.", nameof(member)),
			};
		}

		private static string GetPlainName(IFieldSymbol fieldSymbol)
		{
			var name = fieldSymbol.Name;

			if (name.StartsWith("_"))
			{
				name = name[1..];
			}
			else if (name.StartsWith("s_") || name.StartsWith("t_"))
			{
				name = name[2..];
			}

			return name;
		}

		private static SeparatedSyntaxList<ExpressionSyntax> FormatExpressionList(SeparatedSyntaxList<ExpressionSyntax> expressionList, DocumentOptionSet options)
		{
			var endOfLineTrivia = SyntaxFactory.EndOfLine(options.GetOption(FormattingOptions.NewLine));

			if (expressionList.Count is 1)
			{
				var expression = expressionList[0];
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
