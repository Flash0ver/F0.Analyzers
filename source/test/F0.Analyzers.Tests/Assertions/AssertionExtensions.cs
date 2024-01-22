using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace F0.Tests.Assertions;

internal static class AssertionExtensions
{
	internal static void ShouldNotBeNull<T>([NotNull] this T reference, string because = "", params object[] becauseArgs)
		where T : class?
	{
		_ = reference.Should().NotBeNull(because, becauseArgs);
		Debug.Assert(reference is not null, "Unreachable");
	}
}
