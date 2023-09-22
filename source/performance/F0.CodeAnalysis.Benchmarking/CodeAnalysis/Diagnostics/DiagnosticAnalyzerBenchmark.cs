using System.Collections.Immutable;
using System.Diagnostics;
using F0.Benchmarking.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis.Diagnostics;

public sealed class DiagnosticAnalyzerBenchmark<TDiagnosticAnalyzer> : AnalyzerBenchmark
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
{
	private readonly DiagnosticAnalyzer analyzer;
	private readonly ImmutableArray<DiagnosticAnalyzer> analyzers;

	private SourceText source;
	private Compilation compilation;
	private SyntaxTree syntaxTree;
	private ImmutableArray<Diagnostic> actualDiagnostics;

	internal DiagnosticAnalyzerBenchmark()
	{
		analyzer = new TDiagnosticAnalyzer();
		analyzers = ImmutableArray.Create(analyzer);

		source = null!;
		compilation = null!;
		syntaxTree = null!;
	}

	public void Initialize(string code, LanguageVersion languageVersion)
		=> InitializeAsync(code, languageVersion).GetAwaiter().GetResult();

	public async Task InitializeAsync(string code, LanguageVersion languageVersion)
	{
		var text = code.Untabify();
		source = SourceText.From(text);

		var document = CreateDocument(source, languageVersion);

		var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
		Debug.Assert(compilation is not null, $"Project doesn't support producing compilations: {{ {nameof(Project.SupportsCompilation)} = {document.Project.SupportsCompilation} }}");
		this.compilation = compilation;

		syntaxTree = compilation.SyntaxTrees.Single();
	}

	public void Invoke()
		=> InvokeAsync().GetAwaiter().GetResult();

	public async Task InvokeAsync()
	{
		var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, null, CancellationToken.None);

		var allDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
		actualDiagnostics = allDiagnostics;
	}

	public void Inspect(Diagnostic expectedDiagnostic)
		=> Inspect(ImmutableArray.Create(expectedDiagnostic));

	public void Inspect(ImmutableArray<Diagnostic> expectedDiagnostics)
		=> DiagnosticAssert.AreEqual(expectedDiagnostics, actualDiagnostics);

	public Task InspectAsync(Diagnostic expectedDiagnostic)
	{
		Inspect(ImmutableArray.Create(expectedDiagnostic));

		return Task.CompletedTask;
	}

	public Task InspectAsync(ImmutableArray<Diagnostic> expectedDiagnostics)
	{
		Inspect(expectedDiagnostics);

		return Task.CompletedTask;
	}

	public Diagnostic CreateDiagnostic(Action<DiagnosticBuilder> build)
	{
		var descriptor = analyzer.SupportedDiagnostics.Single();
		var diagnostic = CreateDiagnostic(descriptor, build);
		return diagnostic;
	}

	public ImmutableArray<Diagnostic> CreateDiagnostics(params Action<DiagnosticBuilder>[] builds)
	{
		var descriptor = analyzer.SupportedDiagnostics.Single();
		var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
		foreach (var build in builds)
		{
			var diagnostic = CreateDiagnostic(descriptor, build);
			diagnostics.Add(diagnostic);
		}

		return diagnostics.ToImmutable();
	}

	public ImmutableArray<Diagnostic> CreateDiagnostics(params (string Id, Action<DiagnosticBuilder> Builder)[] builds)
	{
		var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
		foreach (var (id, builder) in builds)
		{
			var descriptor = analyzer.SupportedDiagnostics.Single(d => d.Id.Equals(id, StringComparison.Ordinal));
			var diagnostic = CreateDiagnostic(descriptor, builder);
			diagnostics.Add(diagnostic);
		}

		return diagnostics.ToImmutable();
	}

	private Diagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, Action<DiagnosticBuilder> build)
	{
		var builder = new DiagnosticBuilder(source, syntaxTree);
		build(builder);
		var diagnostic = builder.Build(descriptor);
		return diagnostic;
	}
}
