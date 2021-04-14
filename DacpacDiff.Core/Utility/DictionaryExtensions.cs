using System;
using System.Collections;
using System.Collections.Generic;

namespace DacpacDiff.Core.Utility
{
    internal static class DictionaryExtensions
    {
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
                foreach (var kvp in values)
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
                foreach (var val in values)
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
                foreach (var val in items)
                {
                    dict[keySelector(val)] = valueSelector(val);
                }
            }
            return dict;
        }
    }
}
