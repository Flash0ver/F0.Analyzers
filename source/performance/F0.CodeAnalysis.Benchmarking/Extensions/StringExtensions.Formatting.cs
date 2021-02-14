using System;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace F0.Benchmarking.Extensions
{
	internal static class StringExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "We prefer the CLR type over the language alias for Constructors.")]
		internal static string Untabify(this string code)
		{
			if (code.Contains('\t'))
			{
				code = code.Replace("\t", new String(' ', FormattingOptions.IndentationSize.DefaultValue));
			}

			return code;
		}
	}
}
