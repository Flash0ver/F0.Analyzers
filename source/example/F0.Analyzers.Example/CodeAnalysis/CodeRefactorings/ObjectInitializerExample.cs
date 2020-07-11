using System;
using F0.Analyzers.Example.Dependencies.Dependency;
using F0.Analyzers.Example.Shared;
using Microsoft.Extensions.Hosting;

namespace F0.Analyzers.Example.CodeAnalysis.CodeRefactorings
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "Examples")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Examples")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Examples")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Examples")]
	internal sealed class ObjectInitializerExample
	{
		private readonly string _item1 = "bowl of petunias";
		private static readonly int s_item2 = 42;
		public bool Item3 { get; set; }

		public ValueTuple<int, bool, string> GetBclType()
		{
			var tuple = new ValueTuple<int, bool, string>();
			return tuple;
		}

		public NestedStruct GetNestedStruct()
		{
			return new NestedStruct();
		}

		public NestedClass GetNestedClassWithConstructorArguments()
		{
			return new NestedClass(true, 1);
		}

		public Model GetTypeFromSameAssembly()
		{
			var model = new Model();
			return model;
		}

		public PublicPoco GetPublicTypeFromOtherAssembly()
		{
			var poco = new PublicPoco();
			return poco;
		}

		public InternalPoco GetInternalTypeFromFriendAssembly()
		{
			var poco = new InternalPoco();
			return poco;
		}

		public ConsoleLifetimeOptions GetTypeFromPackage()
		{
			var options = new ConsoleLifetimeOptions();
			return options;
		}

		public NestedStruct GetInitialized()
		{
			return new NestedStruct()
			{
				Field = 1,
				Property = 2
			};
		}

		public ValueTuple<bool, string, int> GetModelWithLocals(int item3)
		{
			var item1 = false;
			const string item2 = "bowl of petunias";
			return new ValueTuple<bool, string, int>();
		}

		public ValueTuple<string, int, bool> GetModelWithMembers()
		{
			return new ValueTuple<string, int, bool>();
		}

		internal struct NestedStruct
		{
			public int Field;

			public int Property { get; set; }

			public void Reset()
				=> (Field, Property) = (0, 0);
		}

		internal class NestedClass
		{
			public static double Average;

			public readonly bool Condition;

			public NestedClass(bool condition, int number)
			{
				(Condition, Number) = (condition, number);
			}

			public static char Character { get; set; }

			public string Text { get; set; }

			public int Number { get; }
		}
	}
}
