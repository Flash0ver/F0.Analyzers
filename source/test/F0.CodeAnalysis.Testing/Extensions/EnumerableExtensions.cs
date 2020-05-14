﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/dotnet/roslyn-sdk
using System;
using System.Collections.Generic;
using System.Linq;

namespace F0.Testing.Extensions
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T> comparer)
		{
			return source.OrderBy(t => t, comparer);
		}

		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare)
		{
			return source.OrderBy(new ComparisonComparer<T>(compare));
		}
	}
}
