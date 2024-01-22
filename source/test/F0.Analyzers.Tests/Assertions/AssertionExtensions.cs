using System.Diagnostics.CodeAnalysis;

namespace F0.Tests.Assertions;

internal static class AssertionExtensions
{
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
	internal static void ShouldNotBeNull<T>([NotNull] this T reference, string because = "", params object[] becauseArgs)
		where T : class?
		=> _ = reference.Should().NotBeNull(because, becauseArgs);
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
}
