// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/dotnet/roslyn-sdk
using System;
using System.Collections.Generic;

namespace F0.Testing.Extensions
{
	internal static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> function)
		{
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = function(key);
				dictionary.Add(key, value);
			}

			return value;
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> function)
		{
			return dictionary.GetOrAdd(key, _ => function());
		}
	}
}
