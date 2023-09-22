using System.Diagnostics.CodeAnalysis;

namespace F0.Testing.CodeAnalysis;

/// <summary>
/// Common document names, used by <see cref="Microsoft.CodeAnalysis.Testing"/>.
/// </summary>
public static class Documents
{
	public const string FilePath = "/0/Test0.cs";

	internal const string Extension = ".cs";

	private const string DocumentFormat = "/{0}/Test{1}.cs";

	[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = ".NET Standard 2.0")]
	public static string CreateDocumentName(int projectIndex, int documentIndex)
		=> String.Format(DocumentFormat, projectIndex + 1, documentIndex);
}
