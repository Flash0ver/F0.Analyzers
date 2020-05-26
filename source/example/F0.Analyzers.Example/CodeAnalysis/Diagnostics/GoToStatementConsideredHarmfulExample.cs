using System;

namespace F0.Analyzers.Example.CodeAnalysis.Diagnostics
{
	internal sealed class GoToStatementConsideredHarmfulExample
	{
		public void GotoStatement()
		{
		Label:
			Console.WriteLine("GOTO Considered Harmful");
			goto Label;
		}

		public void GotoCaseStatement(int number)
		{
			switch (number)
			{
				case 1:
					Console.WriteLine("goto case 2");
					goto case 2;
				case 2:
					Console.WriteLine("goto case 1");
					goto case 1;
				default:
					Console.WriteLine("break");
					break;
			}
		}

		public void GotoDefaultStatement(int number)
		{
			switch (number)
			{
				case 0:
					Console.WriteLine("goto default");
					goto default;
				default:
					Console.WriteLine("break");
					break;
			}
		}
	}
}
