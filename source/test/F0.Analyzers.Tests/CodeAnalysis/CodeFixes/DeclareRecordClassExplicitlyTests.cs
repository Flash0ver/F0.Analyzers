using F0.CodeAnalysis;
using F0.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;

namespace F0.Tests.CodeAnalysis.CodeFixes;

public class DeclareRecordClassExplicitlyTests
{
	[Fact]
	public void DeclareRecordClassExplicitly_CheckType()
		=> Verify.CodeFix<DeclareRecordClassExplicitly>().Type();

	[Fact]
	public async Task RegisterCodeFixesAsync_WithoutClassOrStructKeyword_RegisterCodeFix()
	{
		var code = """
			using System;

			public record {|#0:Record|};

			public abstract record {|#1:@RecordClass|}<T>
			{
				public T Member { get; init; }
			}

			[Obsolete]
			internal sealed record {|#2:@PositionalRecord|}<T>(int Number, string Text) : RecordClass<T> where T : notnull;
			""";

		var fix = """
			using System;

			public record class Record;

			public abstract record class @RecordClass<T>
			{
				public T Member { get; init; }
			}

			[Obsolete]
			internal sealed record class @PositionalRecord<T>(int Number, string Text) : RecordClass<T> where T : notnull;
			""";

		var expected = new[]
		{
			CreateDiagnostic(0, "Record"),
			CreateDiagnostic(1, "RecordClass"),
			CreateDiagnostic(2, "PositionalRecord"),
		};

		await VerifyAsync(code, expected, fix);
	}

	private static DiagnosticResult CreateDiagnostic(int markupKey, string argument)
	{
		return Verify.Diagnostic<F02001ImplicitRecordClassDeclaration, DeclareRecordClassExplicitly>(DiagnosticIds.F02001)
			.WithMessageFormat("Record class '{0}' is declared implicitly")
			.WithArguments(argument)
			.WithSeverity(DiagnosticSeverity.Warning)
			.WithLocation(markupKey);
	}

	private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, string fix)
		=> Verify.CodeFix<F02001ImplicitRecordClassDeclaration, DeclareRecordClassExplicitly>().CodeActionAsync(code, diagnostics, fix, ReferenceAssemblies.Net.Net50);
}
