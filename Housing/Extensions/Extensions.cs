using System.Collections.Generic;
using System.Diagnostics;

namespace Housing
{
	/// <summary>
	///     Provides extension methods.
	/// </summary>
	public static class Extensions
    {
        /// <summary>
        ///     Gets a value from the specified dictionary, returning a default value if the specified key is not present.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="dictionary">The dictionary, which must not be <c>null</c>.</param>
        /// <param name="key">The key, which must not be <c>null</c>.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, or the default value.</returns>
        public static TValue Get<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            Debug.Assert(dictionary != null, "Dictionary must not be null.");
            Debug.Assert(key != null, "Key must not be null.");

            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
