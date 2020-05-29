using System;
using System.Threading.Tasks;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis;
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

			await Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().NoOpAsync(code);
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
			var expected = Verify.Diagnostic<F00001GoToStatementConsideredHarmful>("F00001")
				.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto Label;"))
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithLocation(9, 3);

			await Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, expected);
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
				Verify.Diagnostic<F00001GoToStatementConsideredHarmful>("F00001")
					.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto case 2;"))
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithLocation(11, 5),
				Verify.Diagnostic<F00001GoToStatementConsideredHarmful>("F00001")
					.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto case 1;"))
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithLocation(14, 5)
			};

			await Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, expected);
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

			var expected = Verify.Diagnostic<F00001GoToStatementConsideredHarmful>("F00001")
				.WithMessage(String.Format("Don't use goto statements: '{0}'", "goto default;"))
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithLocation(11, 5);

			await Verify.DiagnosticAnalyzer<F00001GoToStatementConsideredHarmful>().DiagnosticAsync(code, expected);
		}
	}
}
