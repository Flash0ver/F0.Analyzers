using System.Diagnostics.CodeAnalysis;

namespace F0.Analyzers.Example.CSharp2.Shared
{
	internal sealed class Model
	{
		public int Field;

		public int Property
		{
			get { return backingField; }
			set { backingField = value; }
		}

		[SuppressMessage("Style", "IDE0032:Use auto property", Justification = "Feature 'object initializer' is not available in C# 2.")]
		private int backingField;

		public Model()
		{
			Field = 0;
		}
	}
}
