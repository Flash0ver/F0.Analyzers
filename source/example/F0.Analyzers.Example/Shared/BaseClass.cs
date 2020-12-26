namespace F0.Analyzers.Example.Shared
{
	internal abstract class BaseClass : IInterface
	{
		public int Field;

		int IInterface.Length { get; set; }
		public int Current { get; set; }
		public int Value { get; set; }

		public abstract int Property { get; set; }
	}
}
