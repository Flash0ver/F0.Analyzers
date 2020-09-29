using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace F0.Testing.CodeAnalysis.CodeRefactorings
{
	internal class CodeRefactoringAssertions<TCodeRefactoring>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		private const string LanguageName = LanguageNames.CSharp;

		private const string PositionString = "$$";
		private const string SpanStartString = "[|";
		private const string SpanEndString = "|]";

		public CodeRefactoringAssertions(string testCode, string fixedCode)
		{
			TestCode = testCode;
			FixedCode = fixedCode;
		}

		internal string TestCode { get; }
		internal string FixedCode { get; }
		internal LanguageVersion? LanguageVersion { get; set; }
		internal List<string[]> AdditionalProjects { get; } = new List<string[]>();

		internal async Task RunAsync(CancellationToken cancellationToken = default)
		{
			var document = CreateDocument();
			var span = CreateTextSpan(TestCode);
			var codeActions = new List<CodeAction>();
			var context = new CodeRefactoringContext(document, span, codeActions.Add, cancellationToken);

			var provider = new TCodeRefactoring();
			await provider.ComputeRefactoringsAsync(context).ConfigureAwait(false);

			var codeAction = codeActions.Single();
			var operations = await codeAction.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
			var edit = operations.OfType<ApplyChangesOperation>().Single();
			var changedDocument = edit.ChangedSolution.GetDocument(document.Id);

			var reducedDocument = await Simplifier.ReduceAsync(changedDocument, Simplifier.Annotation, null, cancellationToken).ConfigureAwait(false);
			var formattedDocument = await Formatter.FormatAsync(reducedDocument, Formatter.Annotation, null, cancellationToken).ConfigureAwait(false);

			var actualText = await formattedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
			var actualCode = actualText.ToString();

			if (!actualCode.Equals(FixedCode, StringComparison.InvariantCulture))
			{
				var diff = Diff(FixedCode, actualCode);
				var message = "Expected and actual source text differ: " + Environment.NewLine + diff;
				throw new InvalidOperationException(message);
			}
		}

		private Document CreateDocument()
		{
			var index = 0;
			var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			var parseOptions = LanguageVersion.HasValue
				? new CSharpParseOptions(LanguageVersion.Value, DocumentationMode.Diagnose)
				: new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

			var solution = CreateSolution(TestCode, index, compilationOptions, parseOptions);
			var project = solution.Projects.Single();
			var projectId = project.Id;

			foreach (var additionalProject in AdditionalProjects)
			{
				index++;
				solution = AddProjectToSolution(solution, projectId, additionalProject, index, compilationOptions, parseOptions);
			}

			return solution.Projects.Single(p => p.Id == projectId).Documents.Single();
		}

		private static Solution CreateSolution(string code, int index, CompilationOptions compilationOptions, ParseOptions parseOptions)
		{
			var testProjectName = CreateTestProjectName(index);
			var fullFileName = CreateFullFileName(index);

			var projectId = ProjectId.CreateNewId(testProjectName);
			var documentId = DocumentId.CreateNewId(projectId, fullFileName);

			var sourceText = SanitizeMarkup(code);

			return new AdhocWorkspace()
				.CurrentSolution
				.AddProject(projectId, testProjectName, testProjectName, LanguageName)
				.WithProjectCompilationOptions(projectId, compilationOptions)
				.WithProjectParseOptions(projectId, parseOptions)
				.AddDocument(documentId, fullFileName, sourceText, null, fullFileName);
		}

		private static string CreateTestProjectName(int index)
			=> "TestProject" + index;

		private static string CreateFullFileName(int index)
			=> "/0/Test" + index + ".cs";

		private static string SanitizeMarkup(string markup)
		{
			return markup
				.Replace(PositionString, "")
				.Replace(SpanStartString, "")
				.Replace(SpanEndString, "");
		}

		private static Solution AddProjectToSolution(Solution solution, ProjectId mainProjectId, string[] additionalDocuments, int projectIndex, CompilationOptions compilationOptions, ParseOptions parseOptions)
		{
			var testProjectName = CreateTestProjectName(projectIndex);
			var additionalProjectId = ProjectId.CreateNewId(testProjectName);

			solution = solution
				.AddProject(additionalProjectId, testProjectName, testProjectName, LanguageName)
				.WithProjectCompilationOptions(additionalProjectId, compilationOptions)
				.WithProjectParseOptions(additionalProjectId, parseOptions);

			MetadataReference metadataReference = MetadataReference.CreateFromFile(typeof(InternalsVisibleToAttribute).Assembly.Location);

			for (var documentIndex = 0; documentIndex < additionalDocuments.Length; documentIndex++)
			{
				var additionalDocument = additionalDocuments[documentIndex];

				var fullFileName = CreateFullFileName(documentIndex);
				var additionalDocumentId = DocumentId.CreateNewId(additionalProjectId, fullFileName);

				solution = solution
					.AddDocument(additionalDocumentId, fullFileName, additionalDocument, null, fullFileName)
					.AddMetadataReference(additionalProjectId, metadataReference)
					.AddProjectReference(mainProjectId, new ProjectReference(additionalProjectId));
			}

			return solution;
		}

		private static TextSpan CreateTextSpan(string sourceCode)
		{
			var start = sourceCode.IndexOf(SpanStartString) + SpanStartString.Length;
			var end = sourceCode.IndexOf(SpanEndString);
			var length = end - start;
			start -= SpanStartString.Length;

			return new TextSpan(start, length);
		}

		private static string Diff(string original, string modified)
		{
			var diffText = new StringBuilder();

			var differ = new Differ();
			var diffBuilder = new InlineDiffBuilder(differ);
			var diffModel = diffBuilder.BuildDiffModel(original, modified, false);

			foreach (var diffPiece in diffModel.Lines)
			{
				switch (diffPiece.Type)
				{
					case ChangeType.Inserted:
						diffText.Append("+");
						break;
					case ChangeType.Deleted:
						diffText.Append("-");
						break;
					default:
						diffText.Append(" ");
						break;
				}

				diffText.AppendLine(diffPiece.Text);
			}

			return diffText.ToString();
		}
	}
}
