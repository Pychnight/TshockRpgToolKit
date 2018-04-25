using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Scripting
{
	internal static class ScriptHelpers
	{
		static List<string> defaultImports;

		internal static IEnumerable<Assembly> GetReferences()
		{
			//we piggyback on customnpcs, for now...
			return CustomNpcs.ScriptHelpers.GetReferences();
		}

		internal static IEnumerable<string> GetDefaultImports()
		{
			return defaultImports ?? ( defaultImports = new List<string>()
			{
				"System",
				"System.Collections.Generic",
				"System.Threading.Tasks",
				"Microsoft.Xna.Framework",
				"TShockAPI",
				"Corruption.AreaFunctions",
				"Corruption.EmoteFunctions",
				"Corruption.TimeFunctions",
				"Corruption.TileFunctions",
				"Corruption.PlayerFunctions",
				"Corruption.PlayerCommandFunctions",
				"Corruption.NpcFunctions",
				"Corruption.ItemFunctions",
				"CustomNpcs",
				"CustomNpcs.Invasions",
				"CustomNpcs.Npcs",
				"CustomNpcs.Projectiles",
				"CustomNpcs.NpcFunctions",
				"CustomNpcs.ProjectileFunctions",
				"CustomNpcs.CustomIDFunctions",
				"CustomQuests",
				"CustomQuests.Quests",
				"CustomQuests.QuestFunctions",
				"CustomQuests.Triggers"
			} );
		}
	}
}
