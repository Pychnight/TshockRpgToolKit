using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.ComponentModel;

namespace RpgToolsEditor.Models
{
	/// <summary>
	/// Represents a materials requirement for purchase of a ShopProduct.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class RequiredItem : IEquatable<RequiredItem>, IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Item";

		[Browsable(false)]
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
		///     Gets the item name.
		/// </summary>
		[TypeConverter(typeof(ItemNameStringConverter))]
		[JsonProperty("Item", Order = 0)]
		public string ItemName { get => Name; set => Name = value; }

		/// <summary>
		///     Gets the stack size. 
		/// </summary>
		[JsonProperty(Order = 1)]
		public int StackSize { get; set; } = 1;

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		[JsonProperty("Prefix", Order = 2)]
		public byte PrefixId { get; set; }

		public RequiredItem()
		{
		}

		public RequiredItem(RequiredItem other)
		{
			ItemName = other.ItemName;
			StackSize = other.StackSize;
			PrefixId = other.PrefixId;
		}

		public object Clone() => new RequiredItem(this);

		public bool Equals(RequiredItem other) => ItemName == other.ItemName && PrefixId == other.PrefixId;

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(ItemName))
				return $"<No Item> x {StackSize}";
			else
				return $"{ItemName} x {StackSize}";
		}
	}
}
