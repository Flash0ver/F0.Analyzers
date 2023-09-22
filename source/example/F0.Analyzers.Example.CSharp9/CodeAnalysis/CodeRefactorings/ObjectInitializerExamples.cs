using System.Diagnostics.CodeAnalysis;
using F0.Analyzers.Example.CSharp9.Shared;

namespace F0.Analyzers.Example.CSharp9.CodeAnalysis.CodeRefactorings
{
	internal sealed class ObjectInitializerExamples
	{
		[SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "Examples")]
		public Record RecordType_InitAccessor_TargetTypedNewExpression()
		{
			return new();
		}
	}
}
