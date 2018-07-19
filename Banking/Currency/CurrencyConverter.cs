using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Handles string conversions for a Currency.  
	/// </summary>
	public class CurrencyConverter
	{
		static Regex getQuadNamesRegex = new Regex("[A-Za-z]+", RegexOptions.Compiled);

		public CurrencyDefinition Currency { get; private set; }

		List<CurrencyQuadrant> sortedQuadrants;//sorted, and reversed.
		Regex parseCurrencyRegex;
								
		internal CurrencyConverter(CurrencyDefinition currency)
		{
			Currency = currency;
			generateParser(currency);
		}

		private void generateParser(CurrencyDefinition def)
		{
			var sb = new StringBuilder();
			string regexString = "";

			sortedQuadrants = def.Quadrants.ToList();
			sortedQuadrants.Sort((a, b) =>
			{
				if( a.BaseUnitMultiplier == b.BaseUnitMultiplier )
					return 0;
				else
					return a.BaseUnitMultiplier > b.BaseUnitMultiplier ? 1 : -1;
			});

			sortedQuadrants.Reverse();

			sb.Append($"(-|\\+)?");//pos/neg

			foreach(var quad in sortedQuadrants)
			{
				//value-quad pair
				if(!string.IsNullOrWhiteSpace(quad.Abbreviation))
					sb.Append($"((\\d+)({quad.FullName}|{quad.Abbreviation}))?");
				else
					sb.Append($"((\\d+)({quad.FullName}))?");

				sb.Append(@",?");//optional separator between value/quad 
				//sb.Append(@"(\W*)");//optional separator between value/quad pairs
			}

			regexString = sb.ToString();
			//Debug.Print($"Created {Currency} regex = {regexString}");
					
			parseCurrencyRegex = new Regex(regexString,RegexOptions.Compiled);
		}

		public bool TryParse(string input, out decimal value)
		{
			input = input.Replace(" ", "");//quick hack for inputs like "value quad value quad" 

			var tempValue = 0m;
			var match = parseCurrencyRegex.Match(input);
			var quadMatched = false;
			var sign = 1;
			var quadIndex = 0;
			
			if(match.Success)
			{
				if(match.Groups[1].Success)
				{
					sign = match.Groups[1].Value == "-" ? -1 : 1;
				}

				for(var i=3;i<match.Groups.Count;i+=3)
				{
					var gValue = match.Groups[i];
					var gQuad = match.Groups[i + 1];

					if(gValue.Success && gQuad.Success)
					{
						quadMatched = true;
						var quad = sortedQuadrants[quadIndex];
						var number = gValue.Value;
						var quadValue = long.Parse(number);

						tempValue += quadValue * quad.BaseUnitMultiplier;
					}

					quadIndex++;
				}

				//need to check that a quadrant matched, or else signs (+/-) may result in success.
				if(quadMatched)
				{
					tempValue *= sign;
					value = tempValue;
					return true;
				}
			}

			value = 0m;
			return false;
		}
		
		public string ToString(decimal value) // bool useCommas = false)
		{
			Color color = Color.White;
			return ToStringAndColor(value, ref color);// useCommas);
		}
		
		public string ToStringAndColor(decimal value, ref Color color) //, bool useCommas = false)
		{
			string result = null;
			var colorSelected = false; // we find the first non zero quad, to determine color.
			var emitSpace = false;
			var lastQuad = sortedQuadrants[sortedQuadrants.Count - 1];
			var sb = new StringBuilder(64);
			var sign = Math.Sign(value);
			value = Math.Abs(value);

			if( sign < 0 && value >= 1.0m )
			{
				sb.Append('-');
			}

			foreach( var quad in sortedQuadrants )
			{
				var quadValue = (long)value / quad.BaseUnitMultiplier;

				if( quadValue != 0 ||
						quad == lastQuad && sb.Length < 1 )//we must emit a 0 value if no previous quads emitted anything 
				{
					if( emitSpace )
						sb.Append(" ");

					sb.Append(quadValue);
					sb.Append(" ");
					sb.Append(quad.FullName);
					
					//if( useCommas && i < sortedQuadrants.Count - 1 )//dont emit comma on last quad
					//	sb.Append(", ");
					
					emitSpace = true;

					value = value - ( quadValue * quad.BaseUnitMultiplier );

					if( !colorSelected )
					{
						color = sign < 0 ? quad.LossColor : quad.GainColor;
						colorSelected = true;
					}
				}
			}
			
			result = sb.ToString();
			//Debug.Print($"{Currency} - {result}");
			//Debug.Print("Color:{0:x8}", color.PackedValue);

			return result;
		}

		/// <summary>
		/// Helper method to grab all quadrant suffixes.
		/// </summary>
		/// <param name="input">string containing value-quadrant pairs.</param>
		/// <returns>IList of strings.</returns>
		internal static IList<string> ParseQuadrantNames(string input)
		{
			var results = new List<string>();
			var match = getQuadNamesRegex.Match(input);

			while( match.Success )
			{
				results.Add(match.Value);
				match = match.NextMatch();
			}

			return results;
		}
	}
}
