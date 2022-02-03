namespace F0.Analyzers.Example.CSharp10.CodeAnalysis.CodeFixes
{
	internal sealed class DeclareRecordClassExplicitlyExamples
	{
		public record Record(int Number, string Text);

		public record class RecordClass(int Number, string Text);

		public record struct RecordStruct(int Number, string Text);

		public readonly record struct ReadonlyRecordStruct(int Number, string Text);

		public class Class { }

		public static class StaticClass { }

		public struct Struct { }

		public readonly struct ReadonlyStruct { }
	}
}
