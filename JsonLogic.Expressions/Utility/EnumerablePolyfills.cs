#if !NET6_0_OR_GREATER
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Linq;

/// <summary>
/// Polyfills for Linq methods that are not in netstandard2.0 
/// </summary>
internal static class EnumerablePolyfills
{
	internal static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> self, Func<TSource, TKey> keySelector)
	{
		var minKey = default(TKey);
		var minItem = default(TSource);
		var first = true;
		var comparer = Comparer<TKey>.Default;

		foreach (var item in self)
		{
			var key = keySelector(item);

			if (first)
			{
				first = false;
				minKey = key;
				minItem = item;
			}

			if (comparer.Compare(key, minKey) < 0)
			{
				minKey = key;
				minItem = item;
			}
		}

		return minItem;

	}

	internal static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> self, Func<TSource, TKey> keySelector)
	{
		var maxKey = default(TKey);
		var maxItem = default(TSource);
		var first = true;
		var comparer = Comparer<TKey>.Default;

		foreach (var item in self)
		{
			var key = keySelector(item);

			if (first)
			{
				first = false;
				maxKey = key;
				maxItem = item;
			}

			if (comparer.Compare(key, maxKey) > 0)
			{
				maxKey = key;
				maxItem = item;
			}
		}

		return maxItem;
	}
}
#endif
