using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DacpacDiff.Core.Utility
{
    internal static class CollectionExtensions
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        private static readonly int[] PRIMES = new[] { 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        public static int CalculateHashCode(this IEnumerable<object?> constituents)
        {
            var result = 0;
            var i = 0;
            foreach (var c in constituents.ToArray())
            {
                result = unchecked(result + ((c?.GetHashCode() ?? 0) * PRIMES[i]));
                i = ++i % PRIMES.Length;
            }
            return result;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key, if one exists; otherwise, null.
        /// </summary>
        public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue>? dict, TKey key)
        {
            var value = default(TValue?);
            if (key is null || dict?.TryGetValue(key, out value) != true)
            {
                return default;
            }
            return value;
        }

        /// <summary>
        /// Apply all of the content of <paramref name="values"/> to this dictionary.
        /// Overwrites existing keys.
        /// </summary>
        public static IDictionary<TKey, TValue>? Merge<TKey, TValue>(this IDictionary<TKey, TValue>? dict, IEnumerable<KeyValuePair<TKey, TValue>>? values)
        {
            if (dict is not null && values is not null)
            {
                foreach (var kvp in values.ToArray())
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            return dict;
        }

        /// <summary>
        /// Apply all of the content of <paramref name="values"/> to this dictionary, using a selector to determine the key of each value.
        /// Overwrites existing keys.
        /// </summary>
        public static IDictionary<TKey, TValue>? Merge<TKey, TValue>(this IDictionary<TKey, TValue>? dict, IEnumerable<TValue>? values, Func<TValue, TKey> keySelector)
        {
            if (dict is not null && values is not null)
            {
                foreach (var val in values.ToArray())
                {
                    dict[keySelector(val)] = val;
                }
            }
            return dict;
        }

        /// <summary>
        /// Apply all of the content of <paramref name="items"/> to this dictionary, using selectors to determine the key and value of each item.
        /// Overwrites existing keys.
        /// </summary>
        public static IDictionary<TKey, TValue>? Merge<TItem, TKey, TValue>(this IDictionary<TKey, TValue>? dict, IEnumerable<TItem>? items, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector)
        {
            if (dict is not null && items is not null)
            {
                foreach (var val in items.ToArray())
                {
                    dict[keySelector(val)] = valueSelector(val);
                }
            }
            return dict;
        }
        
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
            => source.Where(v => v is not null).Cast<T>();

        /// <summary>
        /// Get the first value in the collection that matches the predicate, or return false.
        /// </summary>
        public static bool TryGetValue<T>(this IEnumerable<T>? col, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
        {
            value = default;
            if (col is not null)
            {
                value = col.FirstOrDefault(predicate);
            }
            return value is not null;
        }
    }
}
