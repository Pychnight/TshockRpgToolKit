using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms.Design;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.Leveling
{
	/// <summary>
	///     Represents a level definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Level : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Level";

		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty("Name", Order = 0)]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}
		
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
		///     Gets or sets the CurrencyRequired to level up.
		/// </summary>
		[JsonProperty("CurrencyRequired", Order = 3)]
		public string CurrencyRequired { get; set; } = "";

		/// <summary>
		///     Gets the prefix for the level.
		/// </summary>
		[JsonProperty("Prefix", Order = 4)]
		public string Prefix { get; set; } = "";

		/// <summary>
		///     Gets the set of item names allowed.
		/// </summary>
		[DisplayName("ItemsAllowed")]
		[JsonProperty("ItemsAllowed", Order = 5)]
		public BindingList<TerrariaItemStringHolder> ItemNamesAllowed { get; set; } = new BindingList<TerrariaItemStringHolder>();
			
		/// <summary>
		///     Gets the set of permissions granted.
		/// </summary>
		[JsonProperty("PermissionsGranted", Order = 6)]
		public BindingList<StringHolder> PermissionsGranted { get; set; } = new BindingList<StringHolder>();
		
		public Level()
		{
		}

		public Level(Level other)
		{
			Name = other.Name;
			DisplayName = other.DisplayName;
			ExpRequired = other.ExpRequired;
			Prefix = other.Prefix;
			ItemNamesAllowed = other.ItemNamesAllowed.DeepClone();
			PermissionsGranted = other.PermissionsGranted.DeepClone();
			CommandsOnLevelUp = other.CommandsOnLevelUp.DeepClone();
			CommandsOnLevelUpOnce = other.CommandsOnLevelUpOnce.DeepClone();
			CommandsOnLevelDown = other.CommandsOnLevelDown.DeepClone();
		}

		public object Clone()
		{
			return new Level(this);
		}
	}
}
