using System;
using System.Composition;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace F0.Testing.Shared
{
	internal static class Check
	{
		private static readonly string[] cSharp = new string[] { LanguageNames.CSharp };

		internal static void Accessibility(Type type)
			=> Assert.True(type.IsNotPublic, "Analyzer should not be part of the public API");

		internal static void NonInheritable(Type type)
			=> Assert.True(type.IsSealed, "Analyzer should not be inheritable.");

		internal static void SharedAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<SharedAttribute>();

			Assert.False(attribute is null, "Missing 'Shared' attribute.");
			Assert.Null(attribute.SharingBoundary);
		}

		internal static void DiagnosticAnalyzerAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<DiagnosticAnalyzerAttribute>();

			Assert.False(attribute is null, "Missing 'DiagnosticAnalyzer' attribute.");
			Assert.Equal(cSharp, attribute.Languages);
		}

		internal static void ExportCodeFixProviderAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<ExportCodeFixProviderAttribute>();

			Assert.False(attribute is null, "Missing 'ExportCodeFixProvider' attribute.");
			Assert.Equal(type.Name, attribute.Name);
			Assert.Equal(cSharp, attribute.Languages);
		}

		internal static void ExportCodeRefactoringProviderAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<ExportCodeRefactoringProviderAttribute>();

			Assert.False(attribute is null, "Missing 'ExportCodeRefactoringProvider' attribute.");
			Assert.Equal(type.Name, attribute.Name);
			Assert.Equal(cSharp, attribute.Languages);
		}
	}
}
