using F0.Analyzers.Example.CSharp2.Shared;

namespace F0.Analyzers.Example.CSharp2.CodeAnalysis.CodeRefactorings
{
	internal sealed class ObjectInitializerExample
	{
		public Model ObjectInitializerIsNotAvailableInCSharp2()
		{
			return new Model();
		}
	}
}
