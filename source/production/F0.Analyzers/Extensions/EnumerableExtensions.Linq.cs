using System;
using System.Collections.Generic;

namespace F0.Extensions
{
	internal static class EnumerableExtensions
	{
		internal static T? SoleOrDefault<T>(this IEnumerable<T> source)
			where T : class
		{
			T? result = null;

			foreach (var element in source)
			{
				if (result is null)
				{
					result = element;
				}
				else
				{
					result = null;
					break;
				}
			}

			return result;
		}

		internal static T? SoleOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
			where T : class
		{
			T? result = null;

			foreach (var element in source)
			{
				if (!predicate(element))
				{
					continue;
				}

				if (result is null)
				{
					result = element;
				}
				else
				{
					result = null;
					break;
				}
			}

			return result;
		}
	}
}
