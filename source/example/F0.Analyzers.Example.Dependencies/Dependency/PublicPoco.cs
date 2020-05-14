using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace F0.Analyzers.Example.Dependencies.Dependency
{
	public sealed class PublicPoco
	{
		public DateTimeOffset Timestamp { get; set; }

		public IList<int> Collection { get; set; }

		public Dictionary<string, string> Map { get; set; }

		public LoggerFilterOptions Options { get; set; }
	}
}
