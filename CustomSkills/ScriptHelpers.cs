﻿using BooTS;
using Corruption;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	internal static class ScriptHelpers
	{
		static List<string> defaultImports;

		internal static IEnumerable<string> GetDefaultImports()
		{
			return defaultImports ?? (defaultImports = new List<string>()
			{
				"System",
				"System.Collections.Generic",
				"Microsoft.Xna.Framework",
				"TShockAPI",
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
				"CustomNpcs",
				"CustomNpcs.Invasions",
				"CustomNpcs.Npcs",
				"CustomNpcs.Projectiles",
				"CustomNpcs.NpcFunctions",
				"CustomNpcs.ProjectileFunctions",
				"CustomNpcs.CustomIDFunctions"
			});
		}

		internal static IEnumerable<Assembly> GetReferences(bool addCallingAssembly = true)
		{
			var assemblies = new List<Assembly>();

			//var assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			var mscorAss = Assembly.GetAssembly(typeof(object));

#pragma warning disable 612, 618
			var sysAss = Assembly.LoadWithPartialName("System");
			var sysCore = Assembly.LoadWithPartialName("System.Core");
#pragma warning restore 612, 618

			var otapiAss = Assembly.GetAssembly(typeof(Vector2));
			var tshockAss = Assembly.GetAssembly(typeof(TSPlayer));
			var corruptionAss = Assembly.GetAssembly(typeof(AreaFunctions));
			//var pluginAss = Assembly.GetExecutingAssembly();

			assemblies.Add(mscorAss);
			assemblies.Add(sysAss);
			assemblies.Add(otapiAss);
			assemblies.Add(tshockAss);
			assemblies.Add(corruptionAss);

			if(addCallingAssembly)
			{
				var pluginAss = Assembly.GetCallingAssembly();
				assemblies.Add(pluginAss);
			}

			assemblies.AddRange(BooHelpers.GetBooLangAssemblies());

			return assemblies;
		}

		internal static IEnumerable<EnsuredMethodSignature> GetEnsuredMethodSignatures()
		{
			var sigs = new List<EnsuredMethodSignature>()
			{
				new EnsuredMethodSignature("OnLevelUp")
					.AddParameter("player",typeof(TSPlayer)),

				new EnsuredMethodSignature("OnCast")
					.AddParameter("player",typeof(TSPlayer)),

				new EnsuredMethodSignature("OnCharge")
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("completion",typeof(float)),

				new EnsuredMethodSignature("OnFire")
					.AddParameter("player",typeof(TSPlayer))
			};

			return sigs;
		}
	}
}
