using System.Composition;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace F0.Testing.Extensions;

internal static class TypeExtensions
{
	private static readonly string[] cSharp = new string[] { LanguageNames.CSharp };

	internal static void VerifyAccessibility(this Type type)
		=> Assert.True(type.IsNotPublic, "Analyzer should not be part of the public API");

	internal static void VerifyNonInheritable(this Type type)
		=> Assert.True(type.IsSealed, "Analyzer should not be inheritable.");

	internal static void VerifySharedAttribute(this Type type)
	{
		var attribute = type.GetCustomAttribute<SharedAttribute>();

		Assert.False(attribute is null, "Missing 'Shared' attribute.");
		Debug.Assert(attribute is not null);
		Assert.Null(attribute.SharingBoundary);
	}

	internal static void VerifyDiagnosticAnalyzerAttribute(this Type type)
	{
		var attribute = type.GetCustomAttribute<DiagnosticAnalyzerAttribute>();

		Assert.False(attribute is null, "Missing 'DiagnosticAnalyzer' attribute.");
		Debug.Assert(attribute is not null);
		Assert.Equal(cSharp, attribute.Languages);
	}

	internal static void VerifyExportCodeFixProviderAttribute(this Type type)
	{
		var attribute = type.GetCustomAttribute<ExportCodeFixProviderAttribute>();

		Assert.False(attribute is null, "Missing 'ExportCodeFixProvider' attribute.");
		Debug.Assert(attribute is not null);
		Assert.Equal(type.Name, attribute.Name);
		Assert.Equal(cSharp, attribute.Languages);
	}

	internal static void VerifyExportCodeRefactoringProviderAttribute(this Type type)
	{
		var attribute = type.GetCustomAttribute<ExportCodeRefactoringProviderAttribute>();

		Assert.False(attribute is null, "Missing 'ExportCodeRefactoringProvider' attribute.");
		Debug.Assert(attribute is not null);
		Assert.Equal(type.Name, attribute.Name);
		Assert.Equal(cSharp, attribute.Languages);
	}
}
