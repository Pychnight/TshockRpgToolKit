using System;
using System.ComponentModel;
//using Corruption.PluginSupport;
using Newtonsoft.Json;
//using Terraria.ID;

namespace RpgToolsEditor.Models.CustomNpcs
{
	/// <summary>
	///     Represents a loot entry definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class LootEntry //: IValidator
	{
		/// <summary>
		///     Gets the name.
		/// </summary>
		[Description("Loot Entry Properties")]
		[TypeConverter(typeof(ItemNameStringConverter))]
		[JsonProperty(Order = 0)]
		public string Name { get; set; } = "Dirt Block";

		/// <summary>
		///     Gets the minimum stack size.
		/// </summary>
		[Description("Loot Entry Properties")]
		[JsonProperty(Order = 1)]
		public int MinStackSize { get; set; }

		/// <summary>
		///     Gets the maximum stack size.
		/// </summary>
		[Description("Loot Entry Properties")]
		[JsonProperty(Order = 2)]
		public int MaxStackSize { get; set; }

		/// <summary>
		///     Gets the prefix.
		/// </summary>
		[Description("Loot Entry Properties")]
		[JsonProperty(Order = 3)]
		public int Prefix { get; set; }

		/// <summary>
		///     Gets the chance.
		/// </summary>
		[Description("Loot Entry Properties")]
		[JsonProperty(Order = 4)]
		public double Chance { get; set; }

		public LootEntry()
		{
		}

		public LootEntry(LootEntry other)
		{
			Name = other.Name;
			MinStackSize = other.MinStackSize;
			MaxStackSize = other.MaxStackSize;
			Prefix = other.Prefix;
			Chance = other.Chance;
		}
		
	}
}
