using System.Collections.Immutable;
using System.Diagnostics;
using F0.Benchmarking.CodeAnalysis.Diagnostics;
using F0.Benchmarking.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis.Suppressors;

public sealed class DiagnosticSuppressorBenchmark<TDiagnosticSuppressor> : AnalyzerBenchmark
	where TDiagnosticSuppressor : DiagnosticSuppressor, new()
{
	private readonly DiagnosticReporter analyzer;
	private readonly DiagnosticSuppressor suppressor;
	private readonly ImmutableArray<DiagnosticAnalyzer> analyzers;

	private SourceText source;
	private Compilation compilation;
	private SyntaxTree syntaxTree;
	private ImmutableArray<Diagnostic> actualDiagnostics;

	internal DiagnosticSuppressorBenchmark()
	{
		analyzer = new DiagnosticReporter();
		suppressor = new TDiagnosticSuppressor();
		analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer, suppressor);

		source = null!;
		compilation = null!;
		syntaxTree = null!;
	}

	public void Initialize(string code, string? assemblyName, LanguageVersion languageVersion, ImmutableArray<Action<LocationBuilder>> locations)
		=> InitializeAsync(code, assemblyName, languageVersion, locations).GetAwaiter().GetResult();

	public async Task InitializeAsync(string code, string? assemblyName, LanguageVersion languageVersion, ImmutableArray<Action<LocationBuilder>> locations)
	{
		var text = code.Untabify();
		source = SourceText.From(text);

		var document = CreateDocument(source, languageVersion);

		var project = document.Project;
		if (assemblyName is not null)
		{
			project = project.WithAssemblyName(assemblyName);
		}

		var compilation = await project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
		Debug.Assert(compilation is not null, $"Project doesn't support producing compilations: {{ {nameof(Project.SupportsCompilation)} = {project.SupportsCompilation} }}");
		this.compilation = compilation;

		syntaxTree = compilation.SyntaxTrees.Single();

		var locationFactories = CreateLocationFactories(source, locations);
		analyzer.SetSupportedDiagnostics(suppressor, locationFactories);
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

	public ImmutableArray<Action<LocationBuilder>> CreateLocations(Action<LocationBuilder> build)
		=> ImmutableArray.Create(build);

	public ImmutableArray<Action<LocationBuilder>> CreateLocations(params Action<LocationBuilder>[] builds)
		=> ImmutableArray.Create(builds);

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

	private Diagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, Action<DiagnosticBuilder> build)
	{
		var builder = new DiagnosticBuilder(source, syntaxTree);
		build(builder);
		var diagnostic = builder.Build(descriptor);
		return diagnostic;
	}

	private static ImmutableArray<LocationFactory> CreateLocationFactories(SourceText source, ImmutableArray<Action<LocationBuilder>> locations)
	{
		return locations.Select(location =>
		{
			return new LocationFactory(CreateLocation);

			Location CreateLocation(SyntaxTree syntaxTree)
			{
				var builder = new LocationBuilder(source);
				location.Invoke(builder);
				return builder.Build(syntaxTree);
			}
		}).ToImmutableArray();
	}
}
