#nullable disable
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Xunit;

namespace F0.Tests
{
	public class AssemblyInfoTests
	{
		private static readonly Version version = new Version(0, 4, 1, 0);

#if NET472
		static AssemblyInfoTests()
		{
			_ = new F0.CodeAnalysis.Diagnostics.F00001GoToStatementConsideredHarmful();
		}
#endif

		[Fact]
		public void AssemblyInfo_DefaultCulture_English()
		{
			var assembly = GetAnalyzerAssembly();
			var attribute = assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();

			attribute.Should().NotBeNull();
			attribute.CultureName.Should().Be("en");
			attribute.Location.Should().Be(UltimateResourceFallbackLocation.MainAssembly);
		}

		[Fact]
		public void AssemblyInfo_AnalyzerAssemblyHasNeitherPubliclyExposedTypesNorMembers_TheAssemblyIsNotCLSCompliant()
		{
			var assembly = GetAnalyzerAssembly();
			var attribute = assembly.GetCustomAttribute<CLSCompliantAttribute>();

			attribute.Should().NotBeNull();
			attribute.IsCompliant.Should().BeFalse();
		}

		private static Assembly GetAnalyzerAssembly()
		{
			var displayName = $"F0.Analyzers, Version={version}, Culture=neutral, PublicKeyToken=null";

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembly = assemblies.Single(a => a.FullName.Equals(displayName, StringComparison.InvariantCulture));

			return assembly;
		}
	}
}
