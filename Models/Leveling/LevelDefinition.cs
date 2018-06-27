using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms.Design;
using Newtonsoft.Json;

namespace RpgToolsEditor.Models.Leveling
{
	/// <summary>
	///     Represents a level definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class LevelDefinition
	{
		/// <summary>
		///     Gets the list of commands to execute on leveling up.
		/// </summary>
		[JsonProperty("CommandsOnLevelUp", Order = 6)]
		//public List<string> CommandsOnLevelUp { get; set; } = new List<string>();
		public BindingList<StringHolder> CommandsOnLevelUp { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///     Gets the list of commands to execute on leveling up, but only once.
		/// </summary>
		[JsonProperty("CommandsOnLevelUpOnce", Order = 7)]
		public BindingList<StringHolder> CommandsOnLevelUpOnce { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///     Gets the list of commands to execute on leveling down.
		/// </summary>
		[JsonProperty("CommandsOnLevelDown", Order = 8)]
		public BindingList<StringHolder> CommandsOnLevelDown { get; set; } = new BindingList<StringHolder>();
		
		/// <summary>
		///     Gets the display name.
		/// </summary>
		[JsonProperty("DisplayName", Order = 1)]
		public string DisplayName { get; set; }

		/// <summary>
		///     Gets the EXP required to level up.
		/// </summary>
		[JsonProperty("ExpRequired", Order = 2)]
		public long ExpRequired { get; set; }

		/// <summary>
		///     Gets the set of item names allowed.
		/// </summary>
		[JsonProperty("ItemsAllowed", Order = 4)]
		public BindingList<TerrariaItemStringHolder> ItemNamesAllowed { get; set; } = new BindingList<TerrariaItemStringHolder>();

		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty("Name", Order = 0)]
		public string Name { get; set; } = "New Level";

		/// <summary>
		///     Gets the set of permissions granted.
		/// </summary>
		[JsonProperty("PermissionsGranted", Order = 5)]
		public BindingList<StringHolder> PermissionsGranted { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///     Gets the prefix for the level.
		/// </summary>
		[JsonProperty("Prefix", Order = 3)]
		public string Prefix { get; set; } = "";
	}
}
