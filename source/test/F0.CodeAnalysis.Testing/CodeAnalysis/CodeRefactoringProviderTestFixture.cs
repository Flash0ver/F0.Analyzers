// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;

namespace F0.Testing.CodeAnalysis
{
	public abstract class CodeRefactoringProviderTestFixture : CodeActionProviderTestFixture
	{
		private async Task<IEnumerable<CodeAction>> GetRefactoringAsync(Document document, TextSpan span)
		{
			var provider = CreateCodeRefactoringProvider;
			var actions = new List<CodeAction>();
			var context = new CodeRefactoringContext(document, span, (a) => actions.Add(a), CancellationToken.None);
			await provider.ComputeRefactoringsAsync(context).ConfigureAwait(false);
			return actions;
		}

		protected async Task TestNoActionsAsync(string markup)
		{
			if (!markup.Contains('\r'))
			{
				markup = markup.Replace("\n", "\r\n");
			}

			MarkupTestFile.GetSpan(markup, out var code, out var span);

			var document = CreateDocument(code);
			var actions = await GetRefactoringAsync(document, span).ConfigureAwait(false);

			actions.Should().BeNullOrEmpty();
		}

		protected async Task TestAsync(
			string markup,
			string expected,
			int actionIndex = 0,
			bool compareTokens = false)
		{
			if (!markup.Contains('\r'))
			{
				markup = markup.Replace("\n", "\r\n");
			}

			if (!expected.Contains('\r'))
			{
				expected = expected.Replace("\n", "\r\n");
			}

			MarkupTestFile.GetSpan(markup, out var code, out var span);

			var document = CreateDocument(code);
			var actions = await GetRefactoringAsync(document, span).ConfigureAwait(false);

			actions.Should().NotBeNull().And.NotBeEmpty();

			var action = actions.ElementAt(actionIndex);
			action.Should().NotBeNull();

			var edit = action.GetOperationsAsync(CancellationToken.None).Result.OfType<ApplyChangesOperation>().First();
			VerifyDocument(expected, compareTokens, edit.ChangedSolution.GetDocument(document.Id));
		}

		protected abstract CodeRefactoringProvider CreateCodeRefactoringProvider { get; }
	}
}
