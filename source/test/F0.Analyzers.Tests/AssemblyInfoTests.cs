#nullable disable
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using Xunit;

namespace F0.Tests
{
	public class AssemblyInfoTests
	{
		[Fact]
		public void AssemblyInfo_DefaultCulture_English()
		{
			var version = new Version(0, 3, 0, 0);
			var displayName = $"F0.Analyzers, Version={version}, Culture=neutral, PublicKeyToken=null";

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembly = assemblies.Single(a => a.FullName.Equals(displayName, StringComparison.InvariantCulture));

			var attribute = assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();

			Assert.NotNull(attribute);
			Assert.Equal("en", attribute.CultureName);
			Assert.Equal(UltimateResourceFallbackLocation.MainAssembly, attribute.Location);
		}

		[Fact]
		public void AssemblyInfo_AnalyzerAssemblyHasNeitherPubliclyExposedTypesNorMembers_TheAssemblyIsNotCLSCompliant()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			var version = new Version(0, 3, 0, 0);
			var displayName = $"F0.Analyzers, Version={version}, Culture=neutral, PublicKeyToken=null";

			var assembly = assemblies.Single(a => a.FullName.Equals(displayName, StringComparison.InvariantCulture));
			var attribute = assembly.GetCustomAttribute<CLSCompliantAttribute>();

			Assert.NotNull(attribute);
			Assert.False(attribute.IsCompliant);
		}
	}
}
