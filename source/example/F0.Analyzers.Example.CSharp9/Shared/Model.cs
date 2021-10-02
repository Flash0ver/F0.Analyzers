namespace F0.Analyzers.Example.CSharp9.Shared
{
	internal sealed record Record(string Text, int Number, bool Condition)
	{
		internal Record()
			: this("bowl of petunias", 42, true)
		{
		}
	}
}
