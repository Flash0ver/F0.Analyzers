using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace F0.Analyzers.Example.Shared
{
	internal class RecordEqualityComparer : EqualityComparer<Record>
	{
		public static RecordEqualityComparer Instance { get; } = new RecordEqualityComparer();
		public override bool Equals(Record x, Record y) => throw new NotImplementedException();
		public override int GetHashCode([DisallowNull] Record obj) => throw new NotImplementedException();
	}
}
