using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption
{
	public static class MiscFunctions
	{
		private static readonly Random Random = new Random();

		/// <summary>
		///     Gets a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>The number.</returns>
		public static double RandomDouble() => Random.NextDouble();

		/// <summary>
		///     Gets a random integer between the specified values.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum, which must be at least <paramref name="min" />.</param>
		/// <returns>The integer.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
		public static int RandomInt(int min, int max)
		{
			if (max < min)
				throw new ArgumentOutOfRangeException(nameof(max), "Maximum must be at least the minimum.");
			
			return Random.Next(min, max);
		}

		/// <summary>
		///     Returns a random integer in the range of 0 to max, inclusive.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum, which must be at least the minimum.</param>
		/// <returns>The random integer.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
		public static int RandomInt(int max) => RandomInt(0, max);
	}
}
