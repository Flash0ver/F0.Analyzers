namespace F0.Testing.CodeAnalysis;

internal static class SolutionStateExtensions
{
	private const string LanguageName = LanguageNames.CSharp;

	internal static void AddAdditionalProjects(this SolutionState solution, string[][] additionalProjects)
	{
		for (var projectIndex = 0; projectIndex < additionalProjects.Length; projectIndex++)
		{
			var additionalDocuments = additionalProjects[projectIndex];
			var projectName = Projects.CreateProjectName(projectIndex);

			var project = new ProjectState(projectName, LanguageName, String.Empty, Projects.Extension)
			{
				OutputKind = OutputKind.DynamicallyLinkedLibrary,
				DocumentationMode = DocumentationMode.Diagnose,
			};

			for (var documentIndex = 0; documentIndex < additionalDocuments.Length; documentIndex++)
			{
				var sourceText = additionalDocuments[documentIndex];
				var filename = Documents.CreateDocumentName(projectIndex, documentIndex);

				project.Sources.Add((filename, sourceText));
			}

			solution.AdditionalProjects.Add(projectName, project);
			solution.AdditionalProjectReferences.Add(projectName);
		}
	}
}
