using F0.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.CodeAnalysis.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class CA1707IdentifiersShouldNotContainUnderscoresSuppressor : DiagnosticSuppressor
{
	private static readonly SuppressionDescriptor Rule = new(
		"F0CA1707",
		"CA1707",
		"Suppress CA1707 on test methods to allow the best practices naming standard for tests."
	);

	private static readonly SymbolDisplayFormat format = new(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

	public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(Rule);

	public override void ReportSuppressions(SuppressionAnalysisContext context)
	{
		foreach (var diagnostic in context.ReportedDiagnostics)
		{
			ReportSuppression(diagnostic, context);
		}
	}

	private static void ReportSuppression(Diagnostic diagnostic, SuppressionAnalysisContext context)
	{
		var location = diagnostic.Location;
		Debug.Assert(location != Location.None, $"{nameof(Diagnostic)} '{diagnostic}' has no primary location.");

		var syntaxTree = location.SourceTree;
		Debug.Assert(syntaxTree is not null, $"{nameof(Location)} '{location}' is not in a syntax tree.");

		var root = syntaxTree.GetRoot(context.CancellationToken);

		var textSpan = location.SourceSpan;
		Debug.Assert(location.IsInSource, $"{nameof(Location)} '{location}' does not represent a specific location in a source code file.");

		var node = root.FindNode(textSpan);

		if (node is not MethodDeclarationSyntax method)
		{
			return;
		}

		if (method.AttributeLists.Count == 0)
		{
			return;
		}

		var semanticModel = context.GetSemanticModel(syntaxTree);

		if (IsTestMethod(semanticModel, method, context.CancellationToken))
		{
			var suppression = Suppression.Create(Rule, diagnostic);
			context.ReportSuppression(suppression);
		}
	}

	private static bool IsTestMethod(SemanticModel semanticModel, MethodDeclarationSyntax method, CancellationToken cancellationToken)
	{
		if (IsMSTest(semanticModel, method, cancellationToken))
		{
			return true;
		}

		var attributes = method.AttributeLists
			.SelectMany(static list => list.Attributes);

		foreach (var attribute in attributes)
		{
			var typeInfo = semanticModel.GetTypeInfo(attribute, cancellationToken);

			Debug.Assert(typeInfo.Type is not null, $"Expression '{attribute}' does not have a type.");

			if (typeInfo.Type is IErrorTypeSymbol)
			{
				continue;
			}

			Debug.Assert(typeInfo.Type is INamedTypeSymbol, $"Unexpected type '{typeInfo.Type.GetType()}' of {nameof(typeInfo.Type.Kind)} '{typeInfo.Type.Kind}'.");

			if (IsNUnit(typeInfo.Type) || IsXunit(typeInfo.Type))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsMSTest(SemanticModel semanticModel, MethodDeclarationSyntax method, CancellationToken cancellationToken)
	{
		var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
		Debug.Assert(symbol is not null, $"Couldn't retrieve the declared {nameof(IMethodSymbol)} of '{method}'.");

		var hasTestMethodAttribute = false;
		foreach (var attribute in symbol.GetAttributes())
		{
			Debug.Assert(attribute.AttributeClass is not null, $"{nameof(AttributeData)} '{attribute}' has no '{nameof(attribute.AttributeClass)}'.");

			var display = attribute.AttributeClass.ToDisplayString(format);

			if ((display.EqualsOrdinal("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute")
				|| display.EqualsOrdinal("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute"))
				&& attribute.AttributeClass.ContainingAssembly.Identity.Name.StartsWithOrdinal("Microsoft.VisualStudio"))
			{
				hasTestMethodAttribute = true;
				break;
			}
		}

		if (!hasTestMethodAttribute)
		{
			return false;
		}

		var hasTestClassAttribute = false;
		foreach (var attribute in symbol.ContainingType.GetAttributes())
		{
			Debug.Assert(attribute.AttributeClass is not null, $"{nameof(AttributeData)} '{attribute}' has no '{nameof(attribute.AttributeClass)}'.");

			var display = attribute.AttributeClass.ToDisplayString(format);

			if (display.EqualsOrdinal("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute")
				&& attribute.AttributeClass.ContainingAssembly.Identity.Name.StartsWithOrdinal("Microsoft.VisualStudio"))
			{
				hasTestClassAttribute = true;
				break;
			}
		}

		return hasTestClassAttribute;
	}

	private static bool IsNUnit(ITypeSymbol attributeType)
	{
		var display = attributeType.ToString();
		Debug.Assert(display is not null, $"Type '{attributeType}' has no display name.");

		return attributeType.ContainingAssembly.Identity.Name.StartsWithOrdinal("nunit")
			&& (display.EqualsOrdinal("NUnit.Framework.TestAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.TestCaseAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.TestCaseSourceAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.CombinatorialAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.PairwiseAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.SequentialAttribute")
			|| display.EqualsOrdinal("NUnit.Framework.TheoryAttribute"));
	}

	private static bool IsXunit(ITypeSymbol attributeType)
	{
		var display = attributeType.ToString();
		Debug.Assert(display is not null, $"Type '{attributeType}' has no display name.");

		return attributeType.ContainingAssembly.Identity.Name.StartsWithOrdinal("xunit")
			&& (display.EqualsOrdinal("Xunit.FactAttribute")
			|| display.EqualsOrdinal("Xunit.TheoryAttribute"));
	}
}
