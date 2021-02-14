using System;
using System.Data;

namespace F0.Analyzers.Example.CSharp7.Shared
{
	internal sealed class Model
	{
		public GlobalType Global;

		public NamespacedType Namespaced { get; set; }

		public NestedType Nested { get; set; }

		public Tuple<string, Type, DbType> Constructed { get; set; }

		internal Model()
		{
		}

		internal Model(GlobalType global)
		{
			Global = global;
		}

		internal sealed class NestedType
		{
		}
	}

	internal sealed class NamespacedType
	{
	}
}

internal sealed class GlobalType
{
}
