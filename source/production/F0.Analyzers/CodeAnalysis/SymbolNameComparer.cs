using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace F0.CodeAnalysis
{
	internal sealed class SymbolNameComparer : EqualityComparer<ISymbol>
	{
		private SymbolNameComparer()
		{
		}

		internal static SymbolNameComparer Instance { get; } = new SymbolNameComparer();

		public override bool Equals(ISymbol x, ISymbol y)
			=> x.Name.Equals(y.Name);

		public override int GetHashCode(ISymbol obj)
			=> obj.Name.GetHashCode();
	}
}
