using System.Collections.Immutable;
using System.Text;

namespace F0.Benchmarking.CodeAnalysis;

internal static class DiagnosticAssert
{
	internal static void AreEqual(Diagnostic expectedDiagnostic, Diagnostic actualDiagnostic)
	{
		var errors = new StringBuilder();

		AreEqual(expectedDiagnostic, actualDiagnostic, errors);

		if (errors.Length != 0)
		{
			errors.Replace(Environment.NewLine, String.Empty, errors.Length - Environment.NewLine.Length, Environment.NewLine.Length);
			errors.Insert(0, $"Unexpected {nameof(Diagnostic)}:{Environment.NewLine}");
			throw new InvalidOperationException(errors.ToString());
		}
	}

	internal static void AreEqual(ImmutableArray<Diagnostic> expectedDiagnostics, ImmutableArray<Diagnostic> actualDiagnostics)
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

			AreEqual(expectedDiagnostic, actualDiagnostic, errors);

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

	private static void AreEqual(Diagnostic expectedDiagnostic, Diagnostic actualDiagnostic, StringBuilder errors)
	{
		if (expectedDiagnostic.GetType() != actualDiagnostic.GetType() && !expectedDiagnostic.IsSuppressed && !actualDiagnostic.IsSuppressed)
		{
			var message = $"- {nameof(Type)}: Expected '{expectedDiagnostic.GetType()}', but actually is of '{actualDiagnostic.GetType()}'.";
			errors.AppendLine(message);
		}

		if (!expectedDiagnostic.Descriptor.Equals(actualDiagnostic.Descriptor))
		{
			var message = $"- Unexpected {nameof(DiagnosticDescriptor)}:";
			errors.AppendLine(message);
			errors.AppendLine(nameof(DiagnosticDescriptor.Category), expectedDiagnostic.Descriptor.Category, actualDiagnostic.Descriptor.Category);
			errors.AppendLine(nameof(DiagnosticDescriptor.DefaultSeverity), expectedDiagnostic.Descriptor.DefaultSeverity, actualDiagnostic.Descriptor.DefaultSeverity);
			errors.AppendLine(nameof(DiagnosticDescriptor.Description), expectedDiagnostic.Descriptor.Description, actualDiagnostic.Descriptor.Description);
			errors.AppendLine(nameof(DiagnosticDescriptor.HelpLinkUri), expectedDiagnostic.Descriptor.HelpLinkUri, actualDiagnostic.Descriptor.HelpLinkUri);
			errors.AppendLine(nameof(DiagnosticDescriptor.Id), expectedDiagnostic.Descriptor.Id, actualDiagnostic.Descriptor.Id);
			errors.AppendLine(nameof(DiagnosticDescriptor.IsEnabledByDefault), expectedDiagnostic.Descriptor.IsEnabledByDefault, actualDiagnostic.Descriptor.IsEnabledByDefault);
			errors.AppendLine(nameof(DiagnosticDescriptor.MessageFormat), expectedDiagnostic.Descriptor.MessageFormat, actualDiagnostic.Descriptor.MessageFormat);
			errors.AppendLine(nameof(DiagnosticDescriptor.Title), expectedDiagnostic.Descriptor.Title, actualDiagnostic.Descriptor.Title);
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

		if (expectedDiagnostic.IsSuppressed != actualDiagnostic.IsSuppressed)
		{
			var message = $"- {nameof(Diagnostic.IsSuppressed)}: Expected '{expectedDiagnostic.IsSuppressed}', but actually is '{actualDiagnostic.IsSuppressed}'.";
			errors.AppendLine(message);
		}
	}

	private static void AppendLine<T>(this StringBuilder errors, string memberName, T expected, T actual)
		=> _ = errors.AppendLine($"\t- {memberName}: {expected} | {actual}");
}
