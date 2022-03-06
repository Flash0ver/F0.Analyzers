namespace F0.Testing.CodeAnalysis;

/// <summary>
/// Common project names, used by <see cref="Microsoft.CodeAnalysis.Testing"/>.
/// </summary>
public static class Projects
{
	public const string AssemblyName = "TestProject";

	internal const string Extension = ".csproj";

	public static string CreateProjectName(int projectIndex)
		=> AssemblyName + projectIndex;
}
