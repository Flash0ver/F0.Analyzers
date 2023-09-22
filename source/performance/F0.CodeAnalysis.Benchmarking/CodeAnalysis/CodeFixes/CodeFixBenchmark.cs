using System.Collections.Immutable;
using F0.Benchmarking.Extensions;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace F0.Benchmarking.CodeAnalysis.CodeFixes;

public sealed class CodeFixBenchmark<TDiagnosticAnalyzer, TCodeFix> : AnalyzerBenchmark
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	private readonly DiagnosticAnalyzer analyzer;
	private readonly CodeFixProvider provider;

	private Document document;
	private Diagnostic diagnostic;
	private ImmutableArray<CodeActionOperation> operations;

	internal CodeFixBenchmark()
	{
		analyzer = new TDiagnosticAnalyzer();
		provider = new TCodeFix();

		document = null!;
		diagnostic = null!;
	}

	public void Initialize(string code, LanguageVersion languageVersion)
		=> InitializeAsync(code, languageVersion).GetAwaiter().GetResult();

	public async Task InitializeAsync(string code, LanguageVersion languageVersion)
	{
		var text = code.Untabify();
		document = CreateDocument(text, languageVersion);

		var analyzers = ImmutableArray.Create(analyzer);

		var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
		var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, null, CancellationToken.None);
		var allDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

		diagnostic = allDiagnostics.Single();
	}

	public void Invoke()
		=> InvokeAsync().GetAwaiter().GetResult();

	public async Task InvokeAsync()
	{
		var actions = ImmutableArray.CreateBuilder<CodeAction>();
		void registerCodeFix(CodeAction a, ImmutableArray<Diagnostic> d) => actions.Add(a);

		var context = new CodeFixContext(document, diagnostic, registerCodeFix, CancellationToken.None);

		await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

		var codeAction = actions.Single();
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
