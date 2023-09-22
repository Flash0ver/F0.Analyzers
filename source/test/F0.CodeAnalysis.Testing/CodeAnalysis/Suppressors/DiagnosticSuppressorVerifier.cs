using System.Diagnostics;
using System.Reflection;
using F0.Testing.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace F0.Testing.CodeAnalysis.Suppressors;

public sealed class DiagnosticSuppressorVerifier<TDiagnosticSuppressor>
	where TDiagnosticSuppressor : DiagnosticAnalyzer, new()
{
	internal DiagnosticSuppressorVerifier()
	{
	}

	public void Type()
	{
		var type = typeof(TDiagnosticSuppressor);

		type.VerifyAccessibility();
		type.VerifyNonInheritable();
		type.VerifyDiagnosticAnalyzerAttribute();
	}
}

public class DiagnosticSuppressorVerifier<TDiagnosticSuppressor, TDiagnosticAnalyzer>
	where TDiagnosticSuppressor : DiagnosticSuppressor, new()
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
{
	internal DiagnosticSuppressorVerifier()
	{
	}

	public Task SuppressionAsync(string code, IEnumerable<DiagnosticResult> diagnostics, ReferenceAssemblies referenceAssemblies, string[][] additionalProjects)
	{
		var tester = CreateTester(code, referenceAssemblies, additionalProjects, null);

		tester.ExpectedDiagnostics.AddRange(diagnostics);

		return tester.RunAsync(CancellationToken.None);
	}

	public Task SuppressionAsync(string code, IEnumerable<DiagnosticResult> diagnostics, ReferenceAssemblies referenceAssemblies, IEnumerable<Type> metadataReferences)
	{
		var tester = CreateTester(code, referenceAssemblies, null, metadataReferences);

		tester.ExpectedDiagnostics.AddRange(diagnostics);

		return tester.RunAsync(CancellationToken.None);
	}

	public Task SuppressionAsync(string code, IEnumerable<DiagnosticResult> diagnostics, ReferenceAssemblies referenceAssemblies, IEnumerable<Type> metadataReferences, AssemblyName name)
	{
		var tester = CreateTester(code, referenceAssemblies, null, metadataReferences);

		tester.ExpectedDiagnostics.AddRange(diagnostics);

		tester.SolutionTransforms.Add((solution, projectId) =>
		{
			Debug.Assert(name.Name is not null, "No assembly name.");
			solution = solution.WithProjectAssemblyName(projectId, name.Name);

			return solution;
		});

		return tester.RunAsync(CancellationToken.None);
	}

	private static DiagnosticSuppressorTester<TDiagnosticSuppressor, TDiagnosticAnalyzer> CreateTester(string code, ReferenceAssemblies? referenceAssemblies = null, string[][]? additionalProjects = null, IEnumerable<Type>? metadataReferences = null)
	{
		metadataReferences ??= Array.Empty<Type>();

		var tester = new DiagnosticSuppressorTester<TDiagnosticSuppressor, TDiagnosticAnalyzer>(metadataReferences)
		{
			TestCode = code.Untabify(),
		};

		if (referenceAssemblies is not null)
		{
			tester.TestState.ReferenceAssemblies = referenceAssemblies;
		}

		if (additionalProjects is not null)
		{
			tester.TestState.AddAdditionalProjects(additionalProjects);
		}

		return tester;
	}
}
