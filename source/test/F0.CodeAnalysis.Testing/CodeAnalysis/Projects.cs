namespace F0.Testing.CodeAnalysis
{
	/// <summary>
	/// Common project names, used by <see cref="Microsoft.CodeAnalysis.Testing"/>.
	/// </summary>
	public static class Projects
	{
		public static readonly string AssemblyName = "TestProject";

		internal static readonly string Extension = ".csproj";

		public static string CreateProjectName(int projectIndex)
			=> AssemblyName + projectIndex;
	}
}
