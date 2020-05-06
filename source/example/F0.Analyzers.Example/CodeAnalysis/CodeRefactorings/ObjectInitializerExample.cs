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

		public Nested GetNestedType()
		{
			return new Nested();
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

		internal struct Nested
		{
			public int Field;

			public int Property { get; set; }

			public void Reset()
			{
				Field = 0;
				Property = 0;
			}
		}
	}
}
