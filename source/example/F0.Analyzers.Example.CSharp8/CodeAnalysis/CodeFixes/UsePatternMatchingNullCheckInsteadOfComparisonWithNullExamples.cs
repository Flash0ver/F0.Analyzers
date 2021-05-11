using System;

namespace F0.Analyzers.Example.CSharp8.CodeAnalysis.CodeFixes
{
	internal sealed class UsePatternMatchingNullCheckInsteadOfComparisonWithNullExamples
	{
		public void IsNullCheck(string text, Type type)
		{
			_ = text == null;
			_ = text != null;

			_ = type == null;
			_ = type != null;
		}
	}
}
