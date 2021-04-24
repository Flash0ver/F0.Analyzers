using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace F0.CodeAnalysis
{
	internal sealed class SymbolNameComparer : EqualityComparer<ISymbol>
	{
		private SymbolNameComparer()
		{
		}

		internal static IEqualityComparer<ISymbol> Instance { get; } = new SymbolNameComparer();

		public override bool Equals(ISymbol? x, ISymbol? y)
		{
			Debug.Assert(x is not null, $"{nameof(x)} not expected to be null");
			Debug.Assert(y is not null, $"{nameof(y)} not expected to be null");

			return x.Name.Equals(y.Name);
		}

		public override int GetHashCode([DisallowNull] ISymbol obj)
			=> obj.Name.GetHashCode();
	}
}
