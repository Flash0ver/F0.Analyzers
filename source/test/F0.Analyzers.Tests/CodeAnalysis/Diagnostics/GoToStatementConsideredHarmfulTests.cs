using System;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace F0.Tests.CodeAnalysis.Diagnostics
{
	public class GoToStatementConsideredHarmfulTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new GoToStatementConsideredHarmful();

		[Fact]
		public void Initialize_NoGotoStatement_ReportsNoDiagnostic()
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

			VerifyCSharpDiagnostic(code);
		}

		[Fact]
		public void Initialize_GotoStatement_ReportsWarning()
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

			var expected = new DiagnosticResult
			{
				Id = "F00001",
				Message = String.Format("'{0}'", "goto Label;"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 3) }
			};

			VerifyCSharpDiagnostic(code, expected);
		}

		[Fact]
		public void Initialize_GotoCaseStatement_ReportsWarning()
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
				new DiagnosticResult
				{
					Id = "F00001",
					Message = String.Format("'{0}'", "goto case 2;"),
					Severity = DiagnosticSeverity.Warning,
					Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 5) }
				},
				new DiagnosticResult
				{
					Id = "F00001",
					Message = String.Format("'{0}'", "goto case 1;"),
					Severity = DiagnosticSeverity.Warning,
					Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 5) }
				}
			};

			VerifyCSharpDiagnostic(code, expected);
		}

		[Fact]
		public void Initialize_GotoDefaultStatement_ReportsWarning()
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

			var expected = new DiagnosticResult
			{
				Id = "F00001",
				Message = String.Format("'{0}'", "goto default;"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 5) }
			};

			VerifyCSharpDiagnostic(code, expected);
		}
	}
}
