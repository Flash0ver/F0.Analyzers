using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace F0.Analyzers.Example.Dependencies.Dependency
{
	internal sealed class InternalPoco
	{
		internal DateTimeOffset Timestamp { get; set; }

		internal IList<int> Collection { get; set; }

		internal Dictionary<string, string> Map { get; set; }

		internal LoggerFilterOptions Options { get; set; }
	}
}
