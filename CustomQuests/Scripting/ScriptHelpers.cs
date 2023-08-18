using System.Collections.Generic;
using System.Reflection;

namespace CustomQuests.Scripting
{
	internal static class ScriptHelpers
	{
		static List<string> defaultImports;

		internal static IEnumerable<Assembly> GetReferences() =>
			//we piggyback on customnpcs, for now...
			CustomNpcs.ScriptHelpers.GetReferences();

		internal static IEnumerable<string> GetDefaultImports()
		{
			return defaultImports ?? (defaultImports = new List<string>()
			{
				"System",
				"System.Collections.Generic",
				"System.Threading.Tasks",
				"Microsoft.Xna.Framework",
				"TShockAPI",
				"Corruption",
				"Corruption.AreaFunctions",
				"Corruption.EmoteFunctions",
				"Corruption.TimeFunctions",
				"Corruption.TileFunctions",
				"Corruption.SignFunctions",
				"Corruption.PlayerFunctions",
				"Corruption.CommandFunctions",
				"Corruption.NpcFunctions",
				"Corruption.ProjectileFunctions",
				"Corruption.ItemFunctions",
				"Corruption.ChestFunctions",
				"Corruption.MiscFunctions",
				"Corruption.TEdit",
				"CustomNpcs",
				"CustomNpcs.Invasions",
				"CustomNpcs.Npcs",
				"CustomNpcs.Projectiles",
				"CustomNpcs.NpcFunctions",
				"CustomNpcs.ProjectileFunctions",
				"CustomNpcs.CustomIDFunctions",
				"CustomQuests",
				"CustomQuests.Quests",
				"CustomQuests.Triggers"
			});
		}
	}
}
