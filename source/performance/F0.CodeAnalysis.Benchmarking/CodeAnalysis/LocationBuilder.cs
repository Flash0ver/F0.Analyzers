using Microsoft.CodeAnalysis.Text;

namespace F0.Benchmarking.CodeAnalysis;

public sealed class LocationBuilder
{
	private readonly SourceText source;
	private LinePosition start;
	private LinePosition end;

	internal LocationBuilder(SourceText source)
	{
		this.source = source;
	}

	public LocationBuilder With(int line, int column)
		=> With(line, column, line, column);

	public LocationBuilder With(int startLine, int startColumn, int endLine, int endColumn)
	{
		start = new LinePosition(startLine - 1, startColumn - 1);
		end = new LinePosition(endLine - 1, endColumn - 1);

		return this;
	}

	internal Location Build(SyntaxTree syntaxTree)
	{
		var lineSpan = new LinePositionSpan(start, end);
		var textSpan = source.Lines.GetTextSpan(lineSpan);

		return Location.Create(syntaxTree, textSpan);
	}
}
