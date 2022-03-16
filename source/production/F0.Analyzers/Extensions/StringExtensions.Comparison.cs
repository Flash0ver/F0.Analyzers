namespace F0.Extensions;

internal static class StringExtensions
{
	internal static bool EqualsOrdinal(this string left, string right)
		=> left.Equals(right, StringComparison.Ordinal);

	internal static bool StartsWithOrdinal(this string left, string right)
		=> left.StartsWith(right, StringComparison.Ordinal);
}
