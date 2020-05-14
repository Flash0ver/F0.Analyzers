// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/dotnet/roslyn-sdk
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using F0.Testing.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace F0.Testing.CodeAnalysis
{
	public static class MarkupTestFile
	{
		private const string PositionString = "$$";
		private const string SpanStartString = "[|";
		private const string SpanEndString = "|]";
		private const string NamedSpanStartString = "{|";
		private const string NamedSpanEndString = "|}";

		private static readonly Regex s_namedSpanStartRegex = new Regex(@"\{\| ([^:]+) \:",
			RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

		private static void Parse(string input, out string output, out int? position, out IDictionary<string, IList<TextSpan>> spans)
		{
			position = null;
			spans = new Dictionary<string, IList<TextSpan>>();

			var outputBuilder = new StringBuilder();

			var currentIndexInInput = 0;
			var inputOutputOffset = 0;

			// A stack of span starts along with their associated annotation name.  [||] spans simply
			// have empty string for their annotation name.
			var spanStartStack = new Stack<Tuple<int, string>>();

			while (true)
			{
				var matches = new List<Tuple<int, string>>();
				AddMatch(input, PositionString, currentIndexInInput, matches);
				AddMatch(input, SpanStartString, currentIndexInInput, matches);
				AddMatch(input, SpanEndString, currentIndexInInput, matches);
				AddMatch(input, NamedSpanEndString, currentIndexInInput, matches);

				var namedSpanStartMatch = s_namedSpanStartRegex.Match(input, currentIndexInInput);
				if (namedSpanStartMatch.Success)
				{
					matches.Add(Tuple.Create(namedSpanStartMatch.Index, namedSpanStartMatch.Value));
				}

				if (matches.Count == 0)
				{
					// No more markup to process.
					break;
				}

				var orderedMatches = matches.OrderBy((t1, t2) => t1.Item1 - t2.Item1).ToList();
				if (orderedMatches.Count >= 2 &&
					spanStartStack.Count > 0 &&
					matches[0].Item1 == matches[1].Item1 - 1)
				{
					// We have a slight ambiguity with cases like these:
					//
					// [|]    [|}
					//
					// Is it starting a new match, or ending an existing match.  As a workaround, we
					// special case these and consider it ending a match if we have something on the
					// stack already.
					if ((matches[0].Item2 == SpanStartString && matches[1].Item2 == SpanEndString && spanStartStack.Peek().Item2 == String.Empty) ||
						(matches[0].Item2 == SpanStartString && matches[1].Item2 == NamedSpanEndString && spanStartStack.Peek().Item2 != String.Empty))
					{
						orderedMatches.RemoveAt(0);
					}
				}

				// Order the matches by their index
				var firstMatch = orderedMatches.First();

				var matchIndexInInput = firstMatch.Item1;
				var matchString = firstMatch.Item2;

				var matchIndexInOutput = matchIndexInInput - inputOutputOffset;
				outputBuilder.Append(input.Substring(currentIndexInInput, matchIndexInInput - currentIndexInInput));

				currentIndexInInput = matchIndexInInput + matchString.Length;
				inputOutputOffset += matchString.Length;

				switch (matchString.Substring(0, 2))
				{
					case PositionString:
						if (position.HasValue)
						{
							throw new ArgumentException(String.Format("Saw multiple occurrences of {0}", PositionString));
						}

						position = matchIndexInOutput;
						break;

					case SpanStartString:
						spanStartStack.Push(Tuple.Create(matchIndexInOutput, String.Empty));
						break;

					case SpanEndString:
						if (spanStartStack.Count == 0)
						{
							throw new ArgumentException(String.Format("Saw {0} without matching {1}", SpanEndString, SpanStartString));
						}

						if (spanStartStack.Peek().Item2.Length > 0)
						{
							throw new ArgumentException(String.Format("Saw {0} without matching {1}", NamedSpanStartString, NamedSpanEndString));
						}

						PopSpan(spanStartStack, spans, matchIndexInOutput);
						break;

					case NamedSpanStartString:
						var name = namedSpanStartMatch.Groups[1].Value;
						spanStartStack.Push(Tuple.Create(matchIndexInOutput, name));
						break;

					case NamedSpanEndString:
						if (spanStartStack.Count == 0)
						{
							throw new ArgumentException(String.Format("Saw {0} without matching {1}", NamedSpanEndString, NamedSpanStartString));
						}

						if (spanStartStack.Peek().Item2.Length == 0)
						{
							throw new ArgumentException(String.Format("Saw {0} without matching {1}", SpanStartString, SpanEndString));
						}

						PopSpan(spanStartStack, spans, matchIndexInOutput);
						break;

					default:
						throw new InvalidOperationException();
				}
			}

			if (spanStartStack.Count > 0)
			{
				throw new ArgumentException(String.Format("Saw {0} without matching {1}", SpanStartString, SpanEndString));
			}

			// Append the remainder of the string.
			outputBuilder.Append(input.Substring(currentIndexInInput));
			output = outputBuilder.ToString();
		}

		private static void PopSpan(
			Stack<Tuple<int, string>> spanStartStack,
			IDictionary<string, IList<TextSpan>> spans,
			int finalIndex)
		{
			var spanStartTuple = spanStartStack.Pop();

			var span = TextSpan.FromBounds(spanStartTuple.Item1, finalIndex);
			spans.GetOrAdd(spanStartTuple.Item2, () => new List<TextSpan>()).Add(span);
		}

		private static void AddMatch(string input, string value, int currentIndex, List<Tuple<int, string>> matches)
		{
			var index = input.IndexOf(value, currentIndex);
			if (index >= 0)
			{
				matches.Add(Tuple.Create(index, value));
			}
		}

		public static void GetPositionAndSpans(string input, out string output, out int? cursorPositionOpt, out IDictionary<string, IList<TextSpan>> spans)
		{
			Parse(input, out output, out cursorPositionOpt, out spans);
		}

		public static void GetPositionAndSpans(string input, out int? cursorPositionOpt, out IDictionary<string, IList<TextSpan>> spans)
		{
			GetPositionAndSpans(input, out _, out cursorPositionOpt, out spans);
		}

		public static void GetPositionAndSpans(string input, out string output, out int cursorPosition, out IDictionary<string, IList<TextSpan>> spans)
		{
			GetPositionAndSpans(input, out output, out int? cursorPositionOpt, out spans);

			cursorPosition = cursorPositionOpt.Value;
		}

		public static void GetSpans(string input, out string output, out IDictionary<string, IList<TextSpan>> spans)
		{
			GetPositionAndSpans(input, out output, out int? _, out spans);
		}

		public static void GetPositionAndSpans(string input, out string output, out int? cursorPositionOpt, out IList<TextSpan> spans)
		{
			Parse(input, out output, out cursorPositionOpt, out var dictionary);

			spans = dictionary.GetOrAdd(String.Empty, () => new List<TextSpan>());
		}

		public static void GetPositionAndSpans(string input, out int? cursorPositionOpt, out IList<TextSpan> spans)
		{
			GetPositionAndSpans(input, out _, out cursorPositionOpt, out spans);
		}

		public static void GetPositionAndSpans(string input, out string output, out int cursorPosition, out IList<TextSpan> spans)
		{
			GetPositionAndSpans(input, out output, out int? pos, out spans);

			cursorPosition = pos ?? 0;
		}

		public static void GetPosition(string input, out string output, out int cursorPosition)
		{
			GetPositionAndSpans(input, out output, out cursorPosition, out IList<TextSpan> _);
		}

		public static void GetPositionAndSpan(string input, out string output, out int cursorPosition, out TextSpan span)
		{
			GetPositionAndSpans(input, out output, out cursorPosition, out IList<TextSpan> spans);

			span = spans.Single();
		}

		public static void GetSpans(string input, out string output, out IList<TextSpan> spans)
		{
			GetPositionAndSpans(input, out output, out int? _, out spans);
		}

		public static void GetSpan(string input, out string output, out TextSpan span)
		{
			GetSpans(input, out output, out IList<TextSpan> spans);

			span = spans.Single();
		}

		public static string CreateTestFile(string code, int cursor)
		{
			return CreateTestFile(code, (IDictionary<string, IList<TextSpan>>)null, cursor);
		}

		public static string CreateTestFile(string code, IList<TextSpan> spans, int cursor = -1)
		{
			return CreateTestFile(code, new Dictionary<string, IList<TextSpan>> { { String.Empty, spans } }, cursor);
		}

		public static string CreateTestFile(string code, IDictionary<string, IList<TextSpan>> spans, int cursor = -1)
		{
			var sb = new StringBuilder();
			var anonymousSpans = spans.GetOrAdd(String.Empty, () => new List<TextSpan>());

			for (var i = 0; i <= code.Length; i++)
			{
				if (i == cursor)
				{
					sb.Append(PositionString);
				}

				AddSpanString(sb, spans.Where(kvp => kvp.Key != String.Empty), i, start: true);
				AddSpanString(sb, spans.Where(kvp => kvp.Key == String.Empty), i, start: true);
				AddSpanString(sb, spans.Where(kvp => kvp.Key == String.Empty), i, start: false);
				AddSpanString(sb, spans.Where(kvp => kvp.Key != String.Empty), i, start: false);

				if (i < code.Length)
				{
					sb.Append(code[i]);
				}
			}

			return sb.ToString();
		}

		private static void AddSpanString(
			StringBuilder sb,
			IEnumerable<KeyValuePair<string, IList<TextSpan>>> items,
			int position,
			bool start)
		{
			foreach (var kvp in items)
			{
				foreach (var span in kvp.Value)
				{
					if (start && span.Start == position)
					{
						if (kvp.Key == String.Empty)
						{
							sb.Append(SpanStartString);
						}
						else
						{
							sb.Append(NamedSpanStartString);
							sb.Append(kvp.Key);
							sb.Append(':');
						}
					}
					else if (!start && span.End == position)
					{
						if (kvp.Key == String.Empty)
						{
							sb.Append(SpanEndString);
						}
						else
						{
							sb.Append(NamedSpanEndString);
						}
					}
				}
			}
		}
	}
}
