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
		public CurrencyDefinition Currency { get; private set; }

		List<CurrencyQuadrantDefinition> sortedQuadrants;//sorted, and reversed.
		Regex parseRegex;
				
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
				sb.Append($"(\\d+{quad.Abbreviation})?");
			}

			regexString = sb.ToString();
			//Debug.Print($"Created {Currency} regex = {regexString}");
					
			parseRegex = new Regex(regexString,RegexOptions.Compiled);
		}

		public bool TryParse(string input, out decimal value)
		{
			var tempValue = 0m;
			var match = parseRegex.Match(input);
			var quadMatched = false;
			var sign = 1;
			
			if(match.Success)
			{
				if(match.Groups[1].Success)
				{
					sign = match.Groups[1].Value == "-" ? -1 : 1;
				}

				for(var i=2;i<match.Groups.Count;i++)
				{
					var g = match.Groups[i];

					if(g.Success)
					{
						quadMatched = true;
						var quad = sortedQuadrants[i - 2];//since regex groups are offset by 1, and we have to account for sign(+/-) as well.
						var number = g.Value.Replace(quad.Abbreviation, "");
						var quadValue = long.Parse(number);

						tempValue += quadValue * quad.BaseUnitMultiplier;
					}
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

		public string ToString(decimal value)
		{
			Color color = Color.White;
			return ToStringAndColor(value,ref color);
		}

		public string ToStringAndColor(decimal value, ref Color color)
		{
			string result = null;
			var choseColor = false;
			var sb = new StringBuilder();
			var sign = Math.Sign(value);
			value = Math.Abs(value);
			
			if( sign < 0 && value>=1.0m )
			{
				sb.Append('-');
			}

			foreach( var quad in sortedQuadrants )
			{
				var quadValue = (long)value / quad.BaseUnitMultiplier;

				if( quadValue != 0 )
				{
					sb.Append(quadValue);
					sb.Append(quad.CombatText ?? quad.Abbreviation);

					value = value - ( quadValue * quad.BaseUnitMultiplier );

					if(!choseColor)
					{
						color = quad.CombatTextColor;
						choseColor = true;
					}
				}
			}

			if( sb.Length < 1 )
			{
				//ensure we output at least a 0 value.
				var quad = sortedQuadrants.Last();

				sb.Append((long)value);
				sb.Append(quad.CombatText ?? quad.Abbreviation);

				color = quad.CombatTextColor;
			}

			result = sb.ToString();
			//Debug.Print($"{Currency} - {result}");
			//Debug.Print("Color:{0:x8}", color.PackedValue);

			return result;
		}
	}
}
