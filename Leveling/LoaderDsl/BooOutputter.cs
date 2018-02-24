using Leveling.Classes;
using Leveling.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling.LoaderDsl
{
	public static class BooFormatExtensions
	{
		static string boolString(bool value)
		{
			return value ? "true" : "false";
		}
		
		static string quotedStringOrNull(string s)
		{
			if( s != null )
			{
				var escapedS = s.Replace("'", "\\'");//escape quotes
													//escapedS = escapedS.Replace("$", "$$");//escape boo interpolation op
				return $"'{escapedS}'";
			}

			return null;
		}

		static string stringValueArray(IEnumerable<string> strings, bool splitLines = false, bool indent = false)
		{
			var sb = new StringBuilder();
			var comma = false;

			foreach(var s in strings)
			{
				if( comma )
				{
					sb.Append(",");
					if( splitLines )
						sb.Append("\n");
				}

				if( comma && indent )
					sb.Append("\t");

				sb.Append(quotedStringOrNull(s));
				comma = true;
			}
			
			return sb.ToString();
		}

		static string stringArguments(IEnumerable<string> strings)
		{
			const int lengthThreshold = 128;

			var splitLines = false;
			var stringCount = 0;
			var charCount = 0;
			
			foreach(var s in strings)
			{
				charCount += s.Length;
				stringCount++;

				if( stringCount > 1 && charCount > lengthThreshold )
				{
					splitLines = true;
					break;
				}
			}

			var values = stringValueArray(strings,splitLines,splitLines);

			if( splitLines )
				return $"({values})\n";
			else
				return values;
		}

		public static string ToBooString(this ClassDefinition def)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("Name {0}\n", quotedStringOrNull(def.Name));
			sb.AppendFormat("DisplayName {0}\n", quotedStringOrNull(def.DisplayName));
			sb.AppendFormat("DisplayInfo {0}\n", quotedStringOrNull(def.DisplayInfo));
			sb.AppendFormat("PrerequisiteLevelNames {0}\n", stringArguments(def.PrerequisiteLevelNames));
			sb.AppendFormat("PrerequisitePermissions {0}\n", stringArguments(def.PrerequisitePermissions));
			sb.AppendFormat("AllowSwitching {0}\n", boolString(def.AllowSwitching));
			sb.AppendFormat("AllowSwitchingBeforeMastery {0}\n", boolString(def.AllowSwitchingBeforeMastery));
			sb.AppendLine();

			foreach(var level in def.LevelDefinitions)
			{
				var s = level.ToBooString();

				sb.AppendLine(s);
			}
			
			return sb.ToString();
		}

		public static string ToBooString(this LevelDefinition def)
		{
			var sb = new StringBuilder();
			string value = null;
			var indent = "\t";

			sb.AppendFormat("Level {0}:\n", quotedStringOrNull(def.Name));

			sb.Append(indent);
			sb.AppendFormat("DisplayName {0}\n", quotedStringOrNull(def.DisplayName));
			sb.Append(indent);
			sb.AppendFormat("ExpRequired {0}\n", def.ExpRequired);

			value = quotedStringOrNull(def.Prefix);
			if(value!=null)
			{
				sb.Append(indent);
				sb.AppendFormat("Prefix {0}\n", value);
			}

			value = stringArguments(def.CommandsOnLevelUpOnce);
			if(value!=null)
			{
				sb.Append(indent);
				sb.AppendFormat("CommandsOnLevelUpOnce {0}\n", value);
			}

			value = stringArguments(def.CommandsOnLevelUp);
			if(value!=null)
			{
				sb.Append(indent);
				sb.AppendFormat("CommandsOnLevelUp {0}\n", value);
			}

			value = stringArguments(def.CommandsOnLevelDown);
			if(value!=null)
			{
				sb.Append(indent);
				sb.AppendFormat("CommandsOnLevelDown {0}\n", value);
			}

			return sb.ToString();
		}
	}
}
