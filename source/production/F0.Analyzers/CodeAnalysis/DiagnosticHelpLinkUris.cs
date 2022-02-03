namespace F0.CodeAnalysis;

internal static class DiagnosticHelpLinkUris
{
	private const string BaseAddress = "https://github.com/Flash0ver/F0.Analyzers/blob/main/documentation/diagnostics/";
	private const string MarkdownExtension = ".md";
	private const string Anchor = "#";

	internal const string F00001 = BaseAddress + nameof(F00001) + MarkdownExtension;

	internal const string F01001 = BaseAddress + "F0100x" + MarkdownExtension + Anchor + nameof(F01001);
	internal const string F01002 = BaseAddress + "F0100x" + MarkdownExtension + Anchor + nameof(F01002);

	internal const string F02001 = BaseAddress + nameof(F02001) + MarkdownExtension;
}
