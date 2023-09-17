using System.Collections.Immutable;
using F0.Benchmarking.Extensions;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis.CodeRefactorings;

public sealed class CodeRefactoringBenchmark<TCodeRefactoring> : AnalyzerBenchmark
	where TCodeRefactoring : CodeRefactoringProvider, new()
{
	private readonly CodeRefactoringProvider provider;

	private TextSpan span;
	private Document document;
	private ImmutableArray<CodeActionOperation> operations;

	internal CodeRefactoringBenchmark()
	{
		provider = new TCodeRefactoring();

		document = null!;
	}

	public void Initialize(string code, LanguageVersion languageVersion, int line, int column, bool isVerbatimStringLiteral)
		=> Initialize(code, languageVersion, new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column)), isVerbatimStringLiteral);

	internal void Initialize(string code, LanguageVersion languageVersion, LinePosition location, bool isVerbatimStringLiteral)
		=> Initialize(code, languageVersion, new LinePositionSpan(location, location), isVerbatimStringLiteral);

	public void Initialize(string code, LanguageVersion languageVersion, int startLine, int startColumn, int endLine, int endColumn, bool isVerbatimStringLiteral)
		=> Initialize(code, languageVersion, new LinePositionSpan(new LinePosition(startLine, startColumn), new LinePosition(endLine, endColumn)), isVerbatimStringLiteral);

	private void Initialize(string code, LanguageVersion languageVersion, LinePositionSpan span, bool isVerbatimStringLiteral)
	{
		var text = code.Untabify();

		document = CreateDocument(text, languageVersion);
		this.span = GetSpan(text, span, isVerbatimStringLiteral);
	}

	public Task InitializeAsync(string code, LanguageVersion languageVersion, int line, int column, bool isVerbatimStringLiteral)
	{
		Initialize(code, languageVersion, new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column)), isVerbatimStringLiteral);

		return Task.CompletedTask;
	}

	internal Task InitializeAsync(string code, LanguageVersion languageVersion, LinePosition location, bool isVerbatimStringLiteral)
	{
		Initialize(code, languageVersion, new LinePositionSpan(location, location), isVerbatimStringLiteral);

		return Task.CompletedTask;
	}

	public Task InitializeAsync(string code, LanguageVersion languageVersion, int startLine, int startColumn, int endLine, int endColumn, bool isVerbatimStringLiteral)
	{
		Initialize(code, languageVersion, new LinePositionSpan(new LinePosition(startLine, startColumn), new LinePosition(endLine, endColumn)), isVerbatimStringLiteral);

		return Task.CompletedTask;
	}

	public void Invoke()
		=> InvokeAsync().GetAwaiter().GetResult();

	public async Task InvokeAsync()
	{
		var codeActions = new List<CodeAction>();
		Action<CodeAction> registerRefactoring = codeActions.Add;

		var context = new CodeRefactoringContext(document, span, registerRefactoring, CancellationToken.None);

		await provider.ComputeRefactoringsAsync(context).ConfigureAwait(false);

		var codeAction = codeActions.Single();
		operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public void Inspect(string expectedCode)
		=> InspectAsync(expectedCode).GetAwaiter().GetResult();

	public async Task InspectAsync(string expectedCode)
	{
		var expectedText = expectedCode.Untabify();

		var edit = operations.OfType<ApplyChangesOperation>().Single();
		var changedDocument = edit.ChangedSolution.GetDocument(document.Id);

		var reducedDocument = await Simplifier.ReduceAsync(changedDocument, Simplifier.Annotation, null, CancellationToken.None).ConfigureAwait(false);
		var formattedDocument = await Formatter.FormatAsync(reducedDocument, Formatter.Annotation, null, CancellationToken.None).ConfigureAwait(false);

		var actualText = await formattedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false);
		var actualCode = actualText.ToString();

		if (!actualCode.Equals(expectedText, StringComparison.Ordinal))
		{
			var message = $"""
				Unexpected result:
				```cs
				{actualCode}
				```
				""";
			throw new InvalidOperationException(message);
		}
	}
}
