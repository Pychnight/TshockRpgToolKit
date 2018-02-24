using Leveling.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling.LoaderDsl
{
	public static class LevelDsl
	{
		internal static LevelDefinition LevelDefinition { get; set; }

		public static void ExpRequired(int exp)
		{
			LevelDefinition.ExpRequired = exp;
		}

		public static void Prefix() { }

		public static void Prefix(string prefix)
		{
			LevelDefinition.Prefix = prefix;// ?? LevelDefinition.Prefix;
		}

		public static void ItemNamesAllowed(params string[] itemNames)
		{
			foreach( var name in itemNames.Distinct() )
				LevelDefinition.ItemNamesAllowed.Add(name);
		}

		public static void PermissionsGranted(params string[] permissions)
		{
			foreach( var name in permissions.Distinct() )
				LevelDefinition.PermissionsGranted.Add(name);
		}

		public static void CommandsOnLevelUpOnce(params string[] commands)
		{
			foreach( var cmd in commands )
				LevelDefinition.CommandsOnLevelUpOnce.Add(cmd);
		}

		public static void CommandsOnLevelUp(params string[] commands)
		{
			foreach( var cmd in commands )
				LevelDefinition.CommandsOnLevelUp.Add(cmd);
		}

		public static void CommandsOnLevelDown(params string[] commands)
		{
			foreach( var cmd in commands )
				LevelDefinition.CommandsOnLevelDown.Add(cmd);
		}
	}
}
