#nullable disable
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using F0.CodeAnalysis.Diagnostics;

namespace F0.Tests
{
	public class AssemblyInfoTests
	{
		private static readonly Version version = new(0, 8, 0, 0);

		static AssemblyInfoTests()
		{
			EnsureThatAssemblyUnderTestIsLoaded();
		}

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

		/// <summary>
		/// On <strong>.NET Core</strong>, <em>xUnit.net</em> loads the Assembly Under Test (<c>ProjectReference</c>) already automatically,
		/// without using any Type in test code. However, not on <strong>.NET Framework</strong>.
		/// </summary>
		[Conditional("NET472")]
		private static void EnsureThatAssemblyUnderTestIsLoaded()
			=> _ = new F00001GoToStatementConsideredHarmful();
	}
}
