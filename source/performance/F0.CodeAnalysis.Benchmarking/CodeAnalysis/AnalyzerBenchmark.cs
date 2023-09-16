using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis;

public abstract class AnalyzerBenchmark
{
	private const char doubleQuotationMark = '"';

	private const string ProjectName = "BenchmarkProject";
	private const string FileName = "Benchmark";
	private const string LanguageName = LanguageNames.CSharp;
	private const string FileExtension = ".cs";

	internal const string FullFileName = "/0/" + FileName + "0" + FileExtension;

	protected AnalyzerBenchmark()
	{
	}

	protected static TextSpan GetSpan(string code, LinePositionSpan span, bool isVerbatimStringLiteral)
	{
		var zeroBasedSpan = new LinePositionSpan(new LinePosition(span.Start.Line - 1, span.Start.Character - 1), new LinePosition(span.End.Line - 1, span.End.Character - 1));

		var index = 0;
		for (var i = 0; i < zeroBasedSpan.Start.Line; i++)
		{
			index = code.IndexOf(Environment.NewLine, index, StringComparison.Ordinal) + Environment.NewLine.Length;
		}

		var startColumn = isVerbatimStringLiteral
			? zeroBasedSpan.Start.Character - code.Substring(index, zeroBasedSpan.Start.Character).Count(c => c == doubleQuotationMark)
			: zeroBasedSpan.Start.Character;
		index += startColumn;
		var start = index;

		for (var i = zeroBasedSpan.Start.Line; i < zeroBasedSpan.End.Line; i++)
		{
			index = code.IndexOf(Environment.NewLine, index, StringComparison.Ordinal) + Environment.NewLine.Length;
		}

		var endColumn = zeroBasedSpan.Start.Line == zeroBasedSpan.End.Line
			? zeroBasedSpan.End.Character - startColumn
			: zeroBasedSpan.End.Character;
		var end = index + endColumn;
		var length = isVerbatimStringLiteral
			? end - start - code.Substring(index, endColumn).Count(c => c == doubleQuotationMark)
			: end - start;

		return new TextSpan(start, length);
	}

	protected static Document CreateDocument(string code, LanguageVersion languageVersion)
		=> CreateDocument(SourceText.From(code), languageVersion);

	protected static Document CreateDocument(SourceText code, LanguageVersion languageVersion)
	{
		var projectId = CreateProjectId();
		var documentId = CreateDocumentId(projectId);

		var solution = CreateSolution(code, languageVersion, projectId, documentId);

		var document = solution.GetDocument(documentId);

		Debug.Assert(document is not null, $"{nameof(DocumentId)} {documentId} is not an ID of a document that is part of this solution.");
		return document;
	}

	protected static Project CreateProject(string code, LanguageVersion languageVersion)
		=> CreateProject(SourceText.From(code), languageVersion);

	protected static Project CreateProject(SourceText code, LanguageVersion languageVersion)
	{
		var projectId = CreateProjectId();
		var documentId = CreateDocumentId(projectId);

		var solution = CreateSolution(code, languageVersion, projectId, documentId);

		var project = solution.GetProject(projectId);

		Debug.Assert(project is not null, $"{nameof(ProjectId)} {projectId} is not an ID of a project that is part of this solution.");
		return project;
	}

	private static Solution CreateSolution(SourceText code, LanguageVersion languageVersion, ProjectId projectId, DocumentId documentId)
	{
		var workspace = CreateWorkspace();

		var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
		var parseOptions = new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose);

		var solution = workspace
			.CurrentSolution
			.AddProject(projectId, ProjectName, ProjectName, LanguageName)
			.WithProjectCompilationOptions(projectId, compilationOptions)
			.WithProjectParseOptions(projectId, parseOptions)
			.AddDocument(documentId, FullFileName, code, null, FullFileName);

		return solution;
	}

	private static Workspace CreateWorkspace()
		=> new AdhocWorkspace();

	private static ProjectId CreateProjectId()
		=> ProjectId.CreateNewId(ProjectName);

	private static DocumentId CreateDocumentId(ProjectId projectId)
		=> DocumentId.CreateNewId(projectId, FullFileName);
}
