using F0.CodeAnalysis;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;

namespace F0.Tests.CodeAnalysis.Diagnostics;

public class F00001GoToStatementConsideredHarmfulTests
{
	[Fact]
	public void F00001GoToStatementConsideredHarmful_CheckType()
		=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().Type();

	[Fact]
	public async Task Initialize_NoGotoStatement_ReportsNoDiagnostic()
	{
		var code = """
			using System;

			class Class
			{
				void Method()
				{
					Console.WriteLine("GOTO Considered Harmful");
				}
			}
			""";

		await VerifyNoOpAsync(code);
	}

	[Fact]
	public async Task Initialize_GotoStatement_ReportsWarning()
	{
		var code = """
			using System;

			class Class
			{
				void Method()
				{
				Label:
					Console.WriteLine("GOTO Considered Harmful");
					{|#0:goto Label;|}
				}
			}
			""";
		var expected = CreateDiagnostic()
			.WithMessageFormat("Don't use goto statements: '{0}'")
			.WithArguments("goto Label;")
			.WithSeverity(DiagnosticSeverity.Warning)
			.WithLocation(0);

		await VerifyAsync(code, expected);
	}

	[Fact]
	public async Task Initialize_GotoCaseStatement_ReportsWarning()
	{
		var code = """
			using System;

			class Class
			{
				void Method(int number)
				{
					switch (number)
					{
						case 1:
							Console.WriteLine("goto case 2");
							{|#0:goto case 2;|}
						case 2:
							Console.WriteLine("goto case 1");
							{|#1:goto case 1;|}
						default:
							Console.WriteLine("break");
							break;
					}
				}
			}
			""";

		var expected = new[]
		{
			CreateDiagnostic()
				.WithMessageFormat("Don't use goto statements: '{0}'")
				.WithArguments("goto case 2;")
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithLocation(0),
			CreateDiagnostic()
				.WithMessageFormat("Don't use goto statements: '{0}'")
				.WithArguments("goto case 1;")
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithLocation(1)
		};

		await VerifyAsync(code, expected);
	}

	[Fact]
	public async Task Initialize_GotoDefaultStatement_ReportsWarning()
	{
		var code = """
			using System;

			class Class
			{
				void Method(int number)
				{
					switch (number)
					{
						case 0:
							Console.WriteLine("goto default");
							{|#0:goto default;|}
						default:
							Console.WriteLine("break");
							break;
					}
				}
			}
			""";

		var expected = CreateDiagnostic()
			.WithMessageFormat("Don't use goto statements: '{0}'")
			.WithArguments("goto default;")
			.WithSeverity(DiagnosticSeverity.Warning)
			.WithLocation(0);

		await VerifyAsync(code, expected);
	}

	private static DiagnosticResult CreateDiagnostic()
		=> Verify.Diagnostic<F00001GoToStatementConsideredHarmful>(DiagnosticIds.F00001);

	private static Task VerifyAsync(string code, DiagnosticResult diagnostic)
		=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, diagnostic);

	private static Task VerifyAsync(string code, params DiagnosticResult[] diagnostics)
		=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, diagnostics);

	private static Task VerifyNoOpAsync(string code)
		=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().NoOpAsync(code);
}
