using F0.CodeAnalysis;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;

namespace F0.Tests.CodeAnalysis.Diagnostics;

public class F02001ImplicitRecordClassDeclarationTests
{
	[Fact]
	public void F02001ImplicitRecordClassDeclaration_CheckType()
		=> Verify.DiagnosticAnalyzer<F02001ImplicitRecordClassDeclaration>().Type();

	[Fact]
	public async Task Initialize_WithoutRecordDeclaration_ReportNoDiagnostic()
	{
		var code = """
			public class Class { }
			public static class StaticClass { }
			public struct Struct { }
			public readonly struct ReadonlyStruct { }
			""";

		await VerifyNoOpAsync(code);
	}

	[Fact]
	public async Task Initialize_WithRecordStructDeclaration_ReportNoDiagnostic()
	{
		var code = """
			public record class RecordClass();
			public record struct RecordStruct();
			public readonly record struct ReadonlyRecordStruct();
			""";

		await VerifyNoOpAsync(code);
	}

	[Theory]
	[InlineData(LanguageVersion.CSharp9)]
	[InlineData(LanguageVersion.CSharp10)]
	public async Task Initialize_WithRecordDeclaration_ReportDiagnosticIfFeatureRecordStructsIsAvailable(LanguageVersion langVersion)
	{
		var code = """
			using System;

			public record {|#0:Record|};

			public record {|#1:Record_WithParameterList|}();
			public record {|#2:PositionalRecord|}(int Number, string Text);

			public record {|#3:Record_WithTypeParameterList|}<T>;
			public record {|#4:Record_WithConstraintClauses|}<T> where T : notnull;
			public record {|#5:Record_WithBaseList|} : Record;

			public record {|#6:Record_WithMembers|}
			{
				public int Number { get; init; }
				public string Text { get; init; }
				public void Method() { }
			}

			internal sealed record {|#7:Record_WithModifiers|};

			[Obsolete]
			public record {|#8:Record_WithAttributeLists|};

			public record {|#9:@Identifier|};
			""";

		DiagnosticResult[] expected;
		if (langVersion >= LanguageVersion.CSharp10)
		{
			expected = new[]
			{
				CreateDiagnostic(0, "Record"),
				CreateDiagnostic(1, "Record_WithParameterList"),
				CreateDiagnostic(2, "PositionalRecord"),
				CreateDiagnostic(3, "Record_WithTypeParameterList"),
				CreateDiagnostic(4, "Record_WithConstraintClauses"),
				CreateDiagnostic(5, "Record_WithBaseList"),
				CreateDiagnostic(6, "Record_WithMembers"),
				CreateDiagnostic(7, "Record_WithModifiers"),
				CreateDiagnostic(8, "Record_WithAttributeLists"),
				CreateDiagnostic(9, "Identifier"),
			};
		}
		else
		{
			expected = Array.Empty<DiagnosticResult>();
		}

		await VerifyAsync(code, expected, langVersion);
	}

	private static DiagnosticResult CreateDiagnostic(int markupKey, string argument)
	{
		return Verify.Diagnostic<F02001ImplicitRecordClassDeclaration>(DiagnosticIds.F02001)
			.WithMessageFormat("Record class '{0}' is declared implicitly")
			.WithArguments(argument)
			.WithSeverity(DiagnosticSeverity.Warning)
			.WithLocation(markupKey);
	}

	private static Task VerifyNoOpAsync(string code)
		=> Verify.DiagnosticAnalyzer<F02001ImplicitRecordClassDeclaration>().NoOpAsync(code, ReferenceAssemblies.Net.Net50, LanguageVersion.CSharp10);

	private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, LanguageVersion langVersion)
		=> Verify.DiagnosticAnalyzer<F02001ImplicitRecordClassDeclaration>().DiagnosticAsync(code, diagnostics, ReferenceAssemblies.Net.Net50, langVersion);
}
