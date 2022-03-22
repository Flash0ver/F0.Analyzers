#if DEBUG && NETFRAMEWORK
using System.Diagnostics.CodeAnalysis;

namespace F0.Tests.Diagnostics;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Replace Assertion Failed Dialog Box")]
internal sealed class DebugAssertException : Exception
{
	public DebugAssertException(string? message)
		: base(message)
	{
	}
}
#endif
