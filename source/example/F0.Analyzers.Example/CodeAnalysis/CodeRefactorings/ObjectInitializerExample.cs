using System;
using F0.Analyzers.Example.Dependencies.Dependency;
using F0.Analyzers.Example.Shared;
using Microsoft.Extensions.Hosting;

namespace F0.Analyzers.Example.CodeAnalysis.CodeRefactorings
{
	internal sealed class ObjectInitializerExample
	{
		public ValueTuple<bool, int, string> GetBclType()
		{
			var tuple = new ValueTuple<bool, int, string>();
			return tuple;
		}

		public NestedStruct GetNestedStruct()
		{
			return new NestedStruct();
		}

		public NestedClass GetNestedClassWithConstructorArguments()
		{
			return new NestedClass(1);
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

		internal struct NestedStruct
		{
			public int Field;

			public int Property { get; set; }

			public void Reset()
			{
				Field = 0;
				Property = 0;
			}
		}

		internal class NestedClass
		{
			public NestedClass(int number)
			{
				Number = number;
			}

			public string Text { get; set; }

			public int Number { get; }
		}
	}
}
