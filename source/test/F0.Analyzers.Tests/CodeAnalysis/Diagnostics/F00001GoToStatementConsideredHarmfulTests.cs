using System;
using System.Threading.Tasks;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace F0.Tests.CodeAnalysis.Diagnostics
{
	public class F00001GoToStatementConsideredHarmfulTests
	{
		[Fact]
		public async Task Initialize_NoGotoStatement_ReportsNoDiagnostic()
		{
			var code =
@"using System;

class Class
{
	void Method()
	{
		Console.WriteLine(""GOTO Considered Harmful"");
	}
}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task Initialize_GotoStatement_ReportsWarning()
		{
			var code =
@"using System;

class Class
{
	void Method()
	{
	Label:
		Console.WriteLine(""GOTO Considered Harmful"");
		goto Label;
	}
}";
			var expected = CreateDiagnostic("F00001")
				.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto Label;"))
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithSpan(9, 9, 9, 20);

			await VerifyAsync(code, expected);
		}

		[Fact]
		public async Task Initialize_GotoCaseStatement_ReportsWarning()
		{
			var code =
@"using System;

class Class
{
	void Method(int number)
	{
		switch (number)
		{
			case 1:
				Console.WriteLine(""goto case 2"");
				goto case 2;
			case 2:
				Console.WriteLine(""goto case 1"");
				goto case 1;
			default:
				Console.WriteLine(""break"");
				break;
		}
	}
}";

			var expected = new[]
			{
				CreateDiagnostic("F00001")
					.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto case 2;"))
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithSpan(11, 17, 11, 29),
				CreateDiagnostic("F00001")
					.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto case 1;"))
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithSpan(14, 17, 14, 29)
			};

			await VerifyAsync(code, expected);
		}

		[Fact]
		public async Task Initialize_GotoDefaultStatement_ReportsWarning()
		{
			var code =
@"using System;

class Class
{
	void Method(int number)
	{
		switch (number)
		{
			case 0:
				Console.WriteLine(""goto default"");
				goto default;
			default:
				Console.WriteLine(""break"");
				break;
		}
	}
}";

			var expected = CreateDiagnostic("F00001")
				.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto default;"))
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithSpan(11, 17, 11, 30);

			await VerifyAsync(code, expected);
		}

		private static DiagnosticResult CreateDiagnostic(string diagnosticId)
			=> Verify.Diagnostic<F00001GoToStatementConsideredHarmful>(diagnosticId);

		private static Task VerifyAsync(string code, DiagnosticResult diagnostic)
			=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, diagnostic);

		private static Task VerifyAsync(string code, params DiagnosticResult[] diagnostics)
			=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, diagnostics);

		private static Task VerifyNoOpAsync(string code)
			=> Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().NoOpAsync(code);
	}
}
