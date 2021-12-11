using System.Collections.Immutable;
using System.Text;
using F0.Benchmarking.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis.Diagnostics
{
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

			compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);

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
		{
			if (expectedDiagnostics.Length != actualDiagnostics.Length)
			{
				var expected = expectedDiagnostics.Length == 1 ? "diagnostic" : "diagnostics";
				var actual = actualDiagnostics.Length == 1 ? "diagnostic" : "diagnostics";
				var message = $"Expected {expectedDiagnostics.Length} {expected}, but actually found {actualDiagnostics.Length} {actual}.";
				throw new InvalidOperationException(message);
			}

			var sortedDiagnostics = actualDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToImmutableArray();
			var errors = new StringBuilder();

			for (var i = 0; i < expectedDiagnostics.Length; i++)
			{
				var expectedDiagnostic = expectedDiagnostics[i];
				var actualDiagnostic = sortedDiagnostics[i];

				var length = errors.Length;

				if (expectedDiagnostic.GetType() != actualDiagnostic.GetType())
				{
					var message = $"- {nameof(Type)}: Expected '{expectedDiagnostic.GetType()}', but actually is of '{actualDiagnostic.GetType()}'.";
					errors.AppendLine(message);
				}
				if (!expectedDiagnostic.Descriptor.Equals(actualDiagnostic.Descriptor))
				{
					var message = $"- Unexpected {nameof(DiagnosticDescriptor)}:";
					errors.AppendLine(message);
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.Category)}: {expectedDiagnostic.Descriptor.Category} | {actualDiagnostic.Descriptor.Category}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.DefaultSeverity)}: {expectedDiagnostic.Descriptor.DefaultSeverity} | {actualDiagnostic.Descriptor.DefaultSeverity}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.Description)}: {expectedDiagnostic.Descriptor.Description} | {actualDiagnostic.Descriptor.Description}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.HelpLinkUri)}: {expectedDiagnostic.Descriptor.HelpLinkUri} | {actualDiagnostic.Descriptor.HelpLinkUri}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.Id)}: {expectedDiagnostic.Descriptor.Id} | {actualDiagnostic.Descriptor.Id}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.IsEnabledByDefault)}: {expectedDiagnostic.Descriptor.IsEnabledByDefault} | {actualDiagnostic.Descriptor.IsEnabledByDefault}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.MessageFormat)}: {expectedDiagnostic.Descriptor.MessageFormat} | {actualDiagnostic.Descriptor.MessageFormat}");
					errors.AppendLine($"\t- {nameof(DiagnosticDescriptor.Title)}: {expectedDiagnostic.Descriptor.Title} | {actualDiagnostic.Descriptor.Title}");
				}
				if (expectedDiagnostic.GetMessage() != actualDiagnostic.GetMessage())
				{
					var message = $"- Message: Expected '{expectedDiagnostic.GetMessage()}', but actually is '{actualDiagnostic.GetMessage()}'.";
					errors.AppendLine(message);
				}
				if (expectedDiagnostic.Location != actualDiagnostic.Location)
				{
					var message = $"- {nameof(Location)}: Expected '{expectedDiagnostic.Location}', but actually at '{actualDiagnostic.Location}'.";
					errors.AppendLine(message);
				}
				if (expectedDiagnostic.Severity != actualDiagnostic.Severity)
				{
					var message = $"- {nameof(DiagnosticSeverity)}: Expected '{expectedDiagnostic.Severity}', but actually is '{actualDiagnostic.Severity}'.";
					errors.AppendLine(message);
				}
				if (expectedDiagnostic.WarningLevel != actualDiagnostic.WarningLevel)
				{
					var message = $"- {nameof(Diagnostic.WarningLevel)}: Expected '{expectedDiagnostic.WarningLevel}', but actually has '{actualDiagnostic.WarningLevel}'.";
					errors.AppendLine(message);
				}

				if (errors.Length != length)
				{
					errors.Insert(length, $"{nameof(Diagnostic)} #{i + 1}{Environment.NewLine}");
				}
			}

			if (errors.Length != 0)
			{
				errors.Replace(Environment.NewLine, String.Empty, errors.Length - Environment.NewLine.Length, Environment.NewLine.Length);
				errors.Insert(0, $"Unexpected {nameof(Diagnostic)}(s):{Environment.NewLine}");
				throw new InvalidOperationException(errors.ToString());
			}
		}

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
}
