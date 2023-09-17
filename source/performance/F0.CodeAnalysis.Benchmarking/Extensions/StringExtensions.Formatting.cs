using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace F0.Benchmarking.Extensions;

internal static class StringExtensions
{
	[SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "We prefer the CLR type over the language alias for Constructors.")]
	private static readonly string tabString = new String(' ', FormattingOptions.IndentationSize.DefaultValue);

	[SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = ".NET Standard 2.0")]
	internal static string Untabify(this string code)
	{
		if (code.Contains('\t'))
		{
			code = code.Replace("\t", tabString);
		}

		return code;
	}
}
