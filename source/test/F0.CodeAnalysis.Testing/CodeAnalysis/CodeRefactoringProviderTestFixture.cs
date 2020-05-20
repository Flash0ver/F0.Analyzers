// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/dotnet/roslyn-sdk
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace F0.Testing.CodeAnalysis
{
	public abstract class CodeRefactoringProviderTestFixture : CodeActionProviderTestFixture
	{
		protected abstract CodeRefactoringProvider CreateCodeRefactoringProvider { get; }

		protected async Task TestNoActionsAsync(string markup, LanguageVersion languageVersion = LanguageVersion.Latest)
		{
			if (!markup.Contains('\r'))
			{
				markup = markup.Replace("\n", "\r\n");
			}

			MarkupTestFile.GetSpan(markup, out var code, out var span);

			var document = CreateDocument(code, languageVersion);
			var actions = await GetRefactoringAsync(document, span).ConfigureAwait(false);

			actions.Should().BeNullOrEmpty();
		}

		protected Task TestAsync(string markup, string expected, params Type[] types)
		{
			return TestAsync(markup, expected, 0, false, LanguageVersion.Latest, types);
		}

		protected Task TestAsync(string markup, string expected, LanguageVersion languageVersion, params Type[] types)
		{
			return TestAsync(markup, expected, 0, false, languageVersion, types);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "We prefer the CLR type over the language alias for Constructors.")]
		protected async Task TestAsync(
			string markup,
			string expected,
			int actionIndex = 0,
			bool compareTokens = false,
			LanguageVersion languageVersion = LanguageVersion.Latest,
			params Type[] types)
		{
			if (!markup.Contains('\r'))
			{
				markup = markup.Replace("\n", "\r\n");
			}

			if (!expected.Contains('\r'))
			{
				expected = expected.Replace("\n", "\r\n");
			}

			if (markup.Contains('\t'))
			{
				markup = markup.Replace("\t", new String(' ', FormattingOptions.IndentationSize.DefaultValue));
			}

			if (expected.Contains('\t'))
			{
				expected = expected.Replace("\t", new String(' ', FormattingOptions.IndentationSize.DefaultValue));
			}

			MarkupTestFile.GetSpan(markup, out var code, out var span);

			var document = CreateDocument(code, languageVersion, types);
			var actions = await GetRefactoringAsync(document, span).ConfigureAwait(false);

			actions.Should().NotBeNull().And.NotBeEmpty();

			var action = actions.ElementAt(actionIndex);
			action.Should().NotBeNull();

			var edit = (await action.GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>().First();
			await VerifyDocumentAsync(expected, compareTokens, edit.ChangedSolution.GetDocument(document.Id));
		}

		private async Task<IEnumerable<CodeAction>> GetRefactoringAsync(Document document, TextSpan span)
		{
			var provider = CreateCodeRefactoringProvider;
			var actions = new List<CodeAction>();
			var context = new CodeRefactoringContext(document, span, (a) => actions.Add(a), CancellationToken.None);
			await provider.ComputeRefactoringsAsync(context).ConfigureAwait(false);
			return actions;
		}
	}
}
