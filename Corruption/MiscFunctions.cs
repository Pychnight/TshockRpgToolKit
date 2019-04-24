using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		
		private static Regex StripTagsRegex = new Regex(@"\[(?<tag>.+?)(:(?<text>.+?))?\]", RegexOptions.Compiled);

		/// <summary>
		/// Looks for and replaces chat tags within a string.
		/// </summary>
		/// <param name="txt">Input text.</param>
		/// <returns>Transformed text.</returns>
		public static string StripTags(string txt)
		{
			if(txt!=null)
			{
				Match match = null;

				while((match = StripTagsRegex.Match(txt)).Success)
				{
					var tag = match.Groups["tag"].Value;

					if(!string.IsNullOrWhiteSpace(tag))
					{
						//tags that we must replace with the decorated text... 
						switch(tag[0])
						{
							case 'c'://color
							case 'C':
							case 'n'://name
							case 'N':
								var text = match.Groups["text"].Value;
								txt = txt.Replace(match.Value, text);
								continue;
						}
					}

					//for any other tags, we just need to strip them altogether
					txt = txt.Replace(match.Value, "");
				}
			}
			
			return txt;
		}
	}
}
