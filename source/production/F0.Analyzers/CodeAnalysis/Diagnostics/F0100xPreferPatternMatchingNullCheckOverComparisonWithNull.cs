using F0.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace F0.CodeAnalysis.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal sealed class F0100xPreferPatternMatchingNullCheckOverComparisonWithNull : DiagnosticAnalyzer
	{
		private const string Title = "Prefer is pattern to check for null";
		private const string Description = "When an expression is matched against the 'null' literal, the compiler guarantees that neither an overloaded operator nor an overridden method is invoked";

		internal static readonly DiagnosticDescriptor EqualityComparisonRule = new(
			DiagnosticIds.F01001,
			Title,
			"Prefer '{0}' pattern over calling the (potentially) {1} '{2}' {3} to check for {4}",
			DiagnosticCategories.BestPractice,
			DiagnosticSeverity.Warning,
			true,
			Description,
			DiagnosticHelpLinkUris.F01001
		);

		internal static readonly DiagnosticDescriptor IdentityComparisonRule = new(
			DiagnosticIds.F01002,
			Title,
			"Prefer '{0}' pattern over calling the '{1}' {2} to check for {3}",
			DiagnosticCategories.BestPractice,
			DiagnosticSeverity.Info,
			true,
			Description,
			DiagnosticHelpLinkUris.F01002
		);

		private const string ObjectReferenceEquals = "ReferenceEquals";
		private const string IsPattern = "is";
		private const string NegationPattern = "is not";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EqualityComparisonRule, IdentityComparisonRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(ctx => EqualityOperatorAction(ctx, WellKnownMemberNames.EqualityOperatorName, false), SyntaxKind.EqualsExpression);
			context.RegisterSyntaxNodeAction(ctx => EqualityOperatorAction(ctx, WellKnownMemberNames.InequalityOperatorName, true), SyntaxKind.NotEqualsExpression);

			context.RegisterOperationAction(MethodInvocationAction, OperationKind.Invocation);
		}

		private static void EqualityOperatorAction(SyntaxNodeAnalysisContext context, string equalityOperatorName, bool isNegation)
		{
			if (!HasNotPattern(context.Compilation))
			{
				return;
			}

			var expression = (BinaryExpressionSyntax)context.Node;
			Debug.Assert(expression.OperatorToken.Kind() is SyntaxKind.EqualsEqualsToken or SyntaxKind.ExclamationEqualsToken);

			if (HasNonNullableValueTypeOperand(expression, context.SemanticModel, context.CancellationToken))
			{
				return;
			}

			if (TryGetTypeCheckedAgainstNull(expression, context.SemanticModel, context.CancellationToken, out var type))
			{
				var equalityOperators = type.GetMembers(equalityOperatorName);
				if (equalityOperators.IsEmpty)
				{
					Debug.Assert(equalityOperators.Length is 0);
					var diagnostic = CreateIdentityComparisonDiagnostic(expression.GetLocation(), isNegation, expression.OperatorToken);
					context.ReportDiagnostic(diagnostic);
				}
				else
				{
					Debug.Assert(equalityOperators.Length is 1);
					var diagnostic = CreateEqualityComparisonDiagnostic(expression.GetLocation(), isNegation, expression.OperatorToken);
					context.ReportDiagnostic(diagnostic);
				}
			}

			static bool HasNonNullableValueTypeOperand(BinaryExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
			{
				var systemNullableT = semanticModel.Compilation.GetTypeByMetadataName("System.Nullable`1");

				if (IsNonNullableValueType(expression.Left, systemNullableT, semanticModel, cancellationToken))
				{
					return true;
				}

				if (IsNonNullableValueType(expression.Right, systemNullableT, semanticModel, cancellationToken))
				{
					return true;
				}

				if (expression.Left is CastExpressionSyntax left)
				{
					if (IsNonNullableValueType(left.Expression, systemNullableT, semanticModel, cancellationToken))
					{
						return true;
					}
				}

				if (expression.Right is CastExpressionSyntax right)
				{
					if (IsNonNullableValueType(right.Expression, systemNullableT, semanticModel, cancellationToken))
					{
						return true;
					}
				}

				return false;
			}

			static bool TryGetTypeCheckedAgainstNull(BinaryExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [MaybeNullWhen(false)] out ITypeSymbol type)
			{
				var left = false;
				var right = false;
				ITypeSymbol? reference = null;

				if (IsNullLiteralExpression(expression.Left))
				{
					left = true;
					reference = semanticModel.GetTypeInfo(expression.Right, cancellationToken).Type;
				}

				if (IsNullLiteralExpression(expression.Right))
				{
					right = true;
					reference = semanticModel.GetTypeInfo(expression.Left, cancellationToken).Type;
				}

				if (left ^ right)
				{
					Debug.Assert(reference is not null);
					Debug.Assert(reference.IsType);
					type = reference;
					return true;
				}
				else
				{
					type = null;
					return false;
				}
			}
		}

		private void MethodInvocationAction(OperationAnalysisContext context)
		{
			if (!HasNotPattern(context.Compilation))
			{
				return;
			}

			var operation = (IInvocationOperation)context.Operation;

			if (operation.Type.SpecialType is not SpecialType.System_Boolean)
			{
				return;
			}

			var method = operation.TargetMethod;
			Debug.Assert(method.ReturnType.SpecialType is SpecialType.System_Boolean);

			while (method.OverriddenMethod is not null && method.ContainingType.SpecialType is not SpecialType.System_ValueType)
			{
				method = method.OverriddenMethod;
			}

			if (method.Name.Equals(WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal)
				&& operation.Arguments.Length is 1
				&& IsNullLiteralExpression(operation.Arguments[0].Value.Syntax))
			{
				Debug.Assert(method.Parameters.Length is 1);

				if (HasNonNullableValueTypeMemberAccess(operation.Instance, context.Compilation, context.Operation.SemanticModel, context.CancellationToken))
				{
					return;
				}

				var isObject = method.ContainingType.SpecialType is SpecialType.System_Object;
				if (isObject)
				{
					ReportNullCheck(context, operation, true);
					return;
				}

				var isValueType = method.ContainingType.SpecialType is SpecialType.System_ValueType;
				if (isValueType)
				{
					ReportNullCheck(context, operation, false);
					return;
				}

				var systemIEquatableT = context.Compilation.GetTypeByMetadataName("System.IEquatable`1");
				if (IsImplementedEqualsMethod(method, systemIEquatableT))
				{
					ReportNullCheck(context, operation, true);
				}
			}
			else if (method.Name.Equals(WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal)
				&& operation.Arguments.Length is 2
				&& IsNullCheck(operation.Arguments)
				&& !HasNonNullableValueTypeArgument(operation.Arguments, context.Operation.SemanticModel, context.CancellationToken))
			{
				var isObject = method.ContainingType.SpecialType is SpecialType.System_Object;
				if (isObject && method.IsStatic)
				{
					ReportNullCheck(context, operation, false);
					return;
				}

				var systemCollectionsGenericReferenceEqualityComparer = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.ReferenceEqualityComparer");
				var isReferenceEqualityComparer = SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, systemCollectionsGenericReferenceEqualityComparer);
				if (isReferenceEqualityComparer)
				{
					ReportNullCheck(context, operation, false);
					return;
				}

				var systemCollectionsGenericIEqualityComparer = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1");
				if (IsImplementedEqualsMethod(method, systemCollectionsGenericIEqualityComparer))
				{
					ReportNullCheck(context, operation, true);
				}
			}
			else if (method.Name.Equals(ObjectReferenceEquals, StringComparison.Ordinal)
				&& operation.Arguments.Length is 2
				&& IsNullCheck(operation.Arguments)
				&& !HasNonNullableValueTypeArgument(operation.Arguments, context.Operation.SemanticModel, context.CancellationToken))
			{
				var isObject = method.ContainingType.SpecialType is SpecialType.System_Object;
				if (isObject && method.IsStatic)
				{
					ReportNullCheck(context, operation, false);
				}
			}

			static bool HasNonNullableValueTypeMemberAccess(IOperation instance, Compilation compilation, SemanticModel semanticModel, CancellationToken cancellationToken)
			{
				var systemNullableT = compilation.GetTypeByMetadataName("System.Nullable`1");

				if (instance.Syntax is IdentifierNameSyntax identifier)
				{
					Debug.Assert(instance.Kind is OperationKind.ParameterReference);

					if (IsNonNullableValueType(identifier, systemNullableT, semanticModel, cancellationToken))
					{
						return true;
					}
				}
				else if (instance.Syntax is CastExpressionSyntax cast)
				{
					Debug.Assert(instance.Kind is OperationKind.Conversion);

					if (IsNonNullableValueType(cast.Expression, systemNullableT, semanticModel, cancellationToken))
					{
						return true;
					}
				}

				return false;
			}

			static bool IsImplementedEqualsMethod(IMethodSymbol method, INamedTypeSymbol? interfaceType)
			{
				Debug.Assert(interfaceType?.TypeKind is TypeKind.Interface);

				var type = method.ContainingType.OriginalDefinition;
				if (type.Equals(interfaceType, SymbolEqualityComparer.Default))
				{
					return true;
				}

				var interfaces = method.ContainingType.AllInterfaces;
				var @interface = interfaces.SoleOrDefault(i => i.ConstructedFrom.Equals(interfaceType, SymbolEqualityComparer.Default));

				if (@interface is not null)
				{
					var equals = @interface.GetMembers(WellKnownMemberNames.ObjectEquals).SoleOrDefault();
					if (equals is not null)
					{
						var implementation = method.ContainingType.FindImplementationForInterfaceMember(equals);
						if (implementation is not null)
						{
							return true;
						}
					}
				}

				return false;
			}

			static bool IsNullCheck(ImmutableArray<IArgumentOperation> arguments)
			{
				Debug.Assert(arguments.Length is 2);

				var left = false;
				var right = false;

				if (IsNullLiteralExpression(arguments[0].Value.Syntax))
				{
					left = true;
				}

				if (IsNullLiteralExpression(arguments[1].Value.Syntax))
				{
					right = true;
				}

				return left ^ right;
			}

			static bool HasNonNullableValueTypeArgument(ImmutableArray<IArgumentOperation> arguments, SemanticModel semanticModel, CancellationToken cancellationToken)
			{
				var systemNullableT = semanticModel.Compilation.GetTypeByMetadataName("System.Nullable`1");

				foreach (var argument in arguments)
				{
					Debug.Assert(argument.Kind is OperationKind.Argument);

					if (argument.Syntax.IsKind(SyntaxKind.IdentifierName))
					{
						var syntax = (IdentifierNameSyntax)argument.Syntax;
						if (IsNonNullableValueType(syntax, systemNullableT, semanticModel, cancellationToken))
						{
							return true;
						}
					}
					else if (argument.Syntax.IsKind(SyntaxKind.Argument))
					{
						var syntax = (ArgumentSyntax)argument.Syntax;
						if (IsNonNullableValueType(syntax.Expression, systemNullableT, semanticModel, cancellationToken))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		private static bool HasNotPattern(Compilation compilation)
		{
			var cSharpCompilation = (CSharpCompilation)compilation;
			return cSharpCompilation.LanguageVersion >= LanguageVersion.CSharp9;
		}

		private static bool IsNullLiteralExpression(SyntaxNode syntax)
			=> syntax is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

		private static bool IsNonNullableValueType(ExpressionSyntax expression, INamedTypeSymbol? systemNullableT, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			Debug.Assert(systemNullableT is null || systemNullableT.MetadataName.Equals("Nullable`1", StringComparison.Ordinal));

			var type = semanticModel.GetTypeInfo(expression, cancellationToken).Type;
			return type is not null && type is not IErrorTypeSymbol && type.TypeKind is TypeKind.Struct && !type.OriginalDefinition.Equals(systemNullableT, SymbolEqualityComparer.Default);
		}

		private static void ReportNullCheck(OperationAnalysisContext context, IInvocationOperation operation, bool isOverridden)
		{
			if (operation.Parent.Kind is OperationKind.Unary)
			{
				var not = (IUnaryOperation)operation.Parent;

				var diagnostic = isOverridden
					? CreateEqualityComparisonDiagnostic(not.Syntax.GetLocation(), true, operation.TargetMethod)
					: CreateIdentityComparisonDiagnostic(not.Syntax.GetLocation(), true, operation.TargetMethod);
				context.ReportDiagnostic(diagnostic);
			}
			else
			{
				var diagnostic = isOverridden
					? CreateEqualityComparisonDiagnostic(operation.Syntax.GetLocation(), false, operation.TargetMethod)
					: CreateIdentityComparisonDiagnostic(operation.Syntax.GetLocation(), false, operation.TargetMethod);
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static Diagnostic CreateEqualityComparisonDiagnostic(Location location, bool isNegation, SyntaxToken @operator)
		{
			var pattern = isNegation ? NegationPattern : IsPattern;
			var test = isNegation ? "non-null" : "null";

			return Diagnostic.Create(EqualityComparisonRule, location, pattern, "overloaded", @operator.ToString(), "operator", test);
		}

		private static Diagnostic CreateEqualityComparisonDiagnostic(Location location, bool isNegation, IMethodSymbol method)
		{
			var pattern = isNegation ? NegationPattern : IsPattern;
			var test = isNegation ? "non-null" : "null";

			return Diagnostic.Create(EqualityComparisonRule, location, pattern, "overridden", method.Name, "method", test);
		}

		private static Diagnostic CreateIdentityComparisonDiagnostic(Location location, bool isNegation, SyntaxToken @operator)
		{
			var pattern = isNegation ? NegationPattern : IsPattern;
			var test = isNegation ? "non-null" : "null";

			return Diagnostic.Create(IdentityComparisonRule, location, pattern, @operator.ToString(), "operator", test);
		}

		private static Diagnostic CreateIdentityComparisonDiagnostic(Location location, bool isNegation, IMethodSymbol method)
		{
			var pattern = isNegation ? NegationPattern : IsPattern;
			var test = isNegation ? "non-null" : "null";

			return Diagnostic.Create(IdentityComparisonRule, location, pattern, method.Name, "method", test);
		}
	}
}
