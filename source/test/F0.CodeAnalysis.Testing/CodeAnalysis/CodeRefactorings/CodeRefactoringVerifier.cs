﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using F0.Testing.Extensions;
using F0.Testing.Shared;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;

namespace F0.Testing.CodeAnalysis.CodeRefactorings
{
	public class CodeRefactoringVerifier<TCodeRefactoring>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		public void Type()
		{
			var type = typeof(TCodeRefactoring);

			Check.Accessibility(type);
			Check.NonInheritable(type);
			Check.ExportCodeRefactoringProviderAttribute(type);
			Check.SharedAttribute(type);
		}

		public Task NoOpAsync(string code)
		{
			var tester = CreateTester(code);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task NoOpAsync(string code, LanguageVersion languageVersion)
		{
			var tester = CreateTester(code, null, languageVersion);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode)
		{
			var tester = CreateTester(initialCode, expectedCode);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, LanguageVersion languageVersion)
		{
			var tester = CreateTester(initialCode, expectedCode, languageVersion);

			return tester.RunAsync(CancellationToken.None);
		}

		public Task CodeActionAsync(string initialCode, string expectedCode, LanguageVersion languageVersion, IEnumerable<Assembly> assemblies)
		{
			var tester = CreateTester(initialCode, expectedCode, languageVersion);

			foreach (var assembly in assemblies)
			{
				tester.TestState.AdditionalReferences.Add(assembly);
			}

			return tester.RunAsync(CancellationToken.None);
		}

		private static CodeRefactoringTester<TCodeRefactoring> CreateTester(string initialCode, string expectedCode = null, LanguageVersion? languageVersion = null)
		{
			var normalizedInitialCode = initialCode.Untabify();

			var tester = new CodeRefactoringTester<TCodeRefactoring>
			{
				TestCode = normalizedInitialCode,
				FixedCode = expectedCode is null ? normalizedInitialCode : expectedCode.Untabify()
			};

			if (languageVersion.HasValue)
			{
				tester.LanguageVersion = languageVersion;
			}

			return tester;
		}
	}
}
