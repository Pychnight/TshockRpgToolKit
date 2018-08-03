using Boo.Lang;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using BooTS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Leveling.Classes;
using Leveling.Levels;

namespace Leveling.LoaderDsl
{
	public static class ClassDsl
	{
		static ClassDefinition classDef = null;
		internal static ClassDefinition ClassDefinition
		{
			get { return classDef; }
			set { classDef = value; LevelDsl.LevelDefinition = null; }//we better reset level def, for safety
		}
				
		public static void Name(string name)
		{
			if( LevelDsl.LevelDefinition != null )
				LevelDsl.LevelDefinition.Name = name;
			else
				ClassDefinition.Name = name;
		}

		//dummy for empty argument lists...
		public static void DisplayName() { }

		public static void DisplayName(string displayName)
		{
			if( LevelDsl.LevelDefinition != null )
				LevelDsl.LevelDefinition.DisplayName = displayName;
			else
				ClassDefinition.DisplayName = displayName;
		}

		public static void DisplayInfo() { }
		
		public static void DisplayInfo(string displayInfo)
		{
			ClassDefinition.DisplayInfo = displayInfo;
		}

		public static void Level(string levelName, ICallable callable)
		{
			if( levelName == null )
				throw new ArgumentNullException("Level name cannot be null.");

			var def = new LevelDefinition();
			LevelDsl.LevelDefinition = def;

			def.Name = levelName;

			if(callable!=null)
				callable.Call(null);

			LevelDsl.LevelDefinition = null;
			ClassDefinition.LevelDefinitions.Add(def);
		}

		public static void PrerequisiteLevelNames(params string[] levelNames)
		{
			foreach(var name in levelNames.Distinct())
				ClassDefinition.PrerequisiteLevelNames.Add(name);
		}

		public static void PrerequisitePermissions(params string[] permissionNames)
		{
			foreach(var name in permissionNames.Distinct())
				ClassDefinition.PrerequisitePermissions.Add(name);
		}

		public static void AllowSwitching(bool allow)
		{
			ClassDefinition.AllowSwitching = allow;
		}

		public static void AllowSwitchingBeforeMastery(bool allow)
		{
			ClassDefinition.AllowSwitchingBeforeMastery = allow;
		}

		public static void SEconomyCost() { SEconomyCost(0); }

		public static void SEconomyCost(long expCost)
		{
			//ClassDefinition.SEconomyCost = expCost;
		}

		public static void CurrencyType() { CurrencyType(null); }

		public static void CurrencyType(string currencyType)
		{
			ClassDefinition.CurrencyType = currencyType;
		}

		public static void CurrencyCost() { CurrencyCost(null); }

		public static void CurrencyCost(string expCost)
		{
			ClassDefinition.CurrencyCost = expCost;
		}

		public static void CommandsOnClassChangeOnce(params string[] commands)
		{
			foreach( var cmd in commands )
				ClassDefinition.CommandsOnClassChangeOnce.Add(cmd);
		}

		public static void NpcToExpReward() { }

		public static void NpcToExpReward(Hash hash)
		{
			foreach(var k in hash.Keys)
			{
				var key = (string)k;
				ClassDefinition.NpcNameToExpReward[key] = (string)hash[key];
			}
		}
	}
}
