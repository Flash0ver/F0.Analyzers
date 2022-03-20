using System.ComponentModel;

namespace F0.Testing.CodeAnalysis;

internal static class DiagnosticSeverityExtensions
{
	public static ReportDiagnostic ToReportDiagnostic(this DiagnosticSeverity severity)
	{
		return severity switch
		{
			DiagnosticSeverity.Hidden => ReportDiagnostic.Hidden,
			DiagnosticSeverity.Info => ReportDiagnostic.Info,
			DiagnosticSeverity.Warning => ReportDiagnostic.Warn,
			DiagnosticSeverity.Error => ReportDiagnostic.Error,
			_ => throw new InvalidEnumArgumentException(nameof(severity), (int)severity, typeof(DiagnosticSeverity)),
		};
	}
}
