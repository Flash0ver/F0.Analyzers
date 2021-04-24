#if DEBUG && NETFRAMEWORK
using System;

namespace F0.Tests.Diagnostics
{
	internal sealed class DebugAssertException : Exception
	{
		public DebugAssertException(string? message)
			: base(message)
		{
		}
	}
}
#endif
