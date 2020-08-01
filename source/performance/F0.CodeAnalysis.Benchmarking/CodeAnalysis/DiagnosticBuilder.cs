using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis
{
	public class DiagnosticBuilder
	{
		private readonly SourceText source;
		private readonly SyntaxTree syntaxTree;
		private TextSpan span;
		private string[]? messageArguments;

		internal DiagnosticBuilder(SourceText source, SyntaxTree syntaxTree)
		{
			this.source = source;
			this.syntaxTree = syntaxTree;
		}

		public DiagnosticBuilder WithLocation(int line, int column)
			=> WithLocation(line, column, line, column);

		public DiagnosticBuilder WithLocation(int startLine, int startColumn, int endLine, int endColumn)
		{
			var start = new LinePosition(startLine - 1, startColumn - 1);
			var end = new LinePosition(endLine - 1, endColumn - 1);
			var lineSpan = new LinePositionSpan(start, end);

			span = source.Lines.GetTextSpan(lineSpan);

			return this;
		}

		public DiagnosticBuilder WithArguments(string argument)
			=> WithArguments(new string[] { argument });

		public DiagnosticBuilder WithArguments(params string[] arguments)
		{
			messageArguments = arguments;

			return this;
		}

		internal Diagnostic Build(DiagnosticDescriptor descriptor)
		{
			var location = BuildLocation();
			var diagnostic = messageArguments is null
				? Diagnostic.Create(descriptor, location)
				: Diagnostic.Create(descriptor, location, messageArguments);
			return diagnostic;
		}

		private Location BuildLocation()
		{
			var location = Location.Create(syntaxTree, span);
			return location;
		}
	}
}
