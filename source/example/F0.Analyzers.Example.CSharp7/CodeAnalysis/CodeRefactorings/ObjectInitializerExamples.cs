//using System;
using System.Diagnostics.CodeAnalysis;
using F0.Analyzers.Example.CSharp7.Shared;

namespace F0.Analyzers.Example.CSharp7.CodeAnalysis.CodeRefactorings
{
	[SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "Examples")]
	internal sealed class ObjectInitializerExamples
	{
		public Model DefaultLiteralIsNotAvailableInCSharp7()
		{
			return new Model();
		}
	}
}
