﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace F0.Testing.CodeAnalysis
{
	public abstract class CodeActionProviderTestFixture
	{
		protected Document CreateDocument(string code)
		{
			var fileExtension = LanguageName == LanguageNames.CSharp ? ".cs" : throw new NotSupportedException($"{LanguageName} not supported.");

			var projectId = ProjectId.CreateNewId(debugName: "TestProject");
			var documentId = DocumentId.CreateNewId(projectId, debugName: "Test" + fileExtension);

			// find these assemblies in the running process
			string[] simpleNames = { "mscorlib", "System.Core", "System" };

			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => simpleNames.Contains(a.GetName().Name, StringComparer.OrdinalIgnoreCase))
				.Select(a => MetadataReference.CreateFromFile(a.Location));

			return new AdhocWorkspace().CurrentSolution
				.AddProject(projectId, "TestProject", "TestProject", LanguageName)
				.AddMetadataReferences(projectId, references)
				.AddDocument(documentId, "Test" + fileExtension, SourceText.From(code))
				.GetDocument(documentId);
		}

		protected void VerifyDocument(string expected, bool compareTokens, Document document)
		{
			if (compareTokens)
			{
				VerifyTokens(expected, Format(document).ToString());
			}
			else
			{
				VerifyText(expected, document);
			}
		}

		private SyntaxNode Format(Document document)
		{
			var updatedDocument = document.WithSyntaxRoot(document.GetSyntaxRootAsync().Result);
			return Formatter.FormatAsync(Simplifier.ReduceAsync(updatedDocument, Simplifier.Annotation).Result, Formatter.Annotation).Result.GetSyntaxRootAsync().Result;
		}

		private IList<SyntaxToken> ParseTokens(string text)
		{
			return LanguageName == LanguageNames.CSharp
				? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTokens(text).Select(t => t).ToList()
				: throw new NotSupportedException($"{LanguageName} not supported.");
		}

		private void VerifyTokens(string expected, string actual)
		{
			var expectedNewTokens = ParseTokens(expected);
			var actualNewTokens = ParseTokens(actual);

			for (var i = 0; i < Math.Min(expectedNewTokens.Count, actualNewTokens.Count); i++)
			{
				expectedNewTokens[i].ToString().Should().Be(actualNewTokens[i].ToString());
			}

			if (expectedNewTokens.Count != actualNewTokens.Count)
			{
				var expectedDisplay = String.Join(" ", expectedNewTokens.Select(t => t.ToString()));
				var actualDisplay = String.Join(" ", actualNewTokens.Select(t => t.ToString()));

				var message = $"Wrong token count. " +
					$"Expected '{expectedNewTokens.Count}', Actual '{actualNewTokens.Count}', Expected Text: '{expectedDisplay}', Actual Text: '{actualDisplay}'";
				throw new AssertionFailedException(message);
			}

		}

		private void VerifyText(string expected, Document document)
		{
			var actual = Format(document).ToString();
			actual.Should().Be(expected);
		}

		protected abstract string LanguageName { get; }
	}
}
