using System.Collections.Generic;
using System.Diagnostics;
using TShockAPI;

namespace Leveling
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

		/// <summary>
		///     Gets the escaped name for the specified player, suitable for use in commands.
		/// </summary>
		/// <param name="player">The player, which must not be <c>null</c>.</param>
		/// <returns>The escaped name.</returns>
		public static string GetEscapedName(this TSPlayer player)
		{
			Debug.Assert(player != null, "Player must not be null.");

			// First, we need to replace all instances of \\ with \\\\. This is because otherwise, the TShock command
			// system would treat the \\ as an escaped \. Then we need to replace \" with \\" and \(space) with
			// \\(space). Then we escape quotes.
			var name = player.Name.Replace(@"\\", @"\\\\");
			name = name.Replace(@"\""", @"\\""");
			name = name.Replace(@"\ ", @"\\ ");
			name = name.Replace(@"""", @"\""");
			return name;
		}
	}
}
