using RpgToolsEditor.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ShopItem : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New ShopItem";

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
		[DisplayName("Item")]
		[JsonProperty("Item", Order = 0)]
		//public string ItemName { get => Name; set => Name = value; }
		public string ItemName { get => Name; set => Name = value; } //= "New ShopItem";

		/// <summary>
		///     Gets the stack size. A value of -1 indicates unlimited.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int StackSize { get; set; } = 1;

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		[JsonProperty("Prefix", Order = 2)]
		public byte PrefixId { get; set; }

		string unitPrice;

		/// <summary>
		///     Gets the unit price.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string UnitPrice
		{
			get => unitPrice;
			set
			{
				unitPrice = value;

				//if( !NpcShopsPlugin.Instance.Currency.GetCurrencyConverter().TryParse(value, out var result) )
				//{
				//	Debug.Print($"Failed to parse UnitPrice for NpcShop Item '{ItemName}.' Setting to 1.");
				//	UnitPriceMoney = 1;
				//}
				//else
				//{
				//	UnitPriceMoney = result;
				//}
			}
		}

		/// <summary>
		///		Gets the numeric value of the UnitPrice string.
		/// </summary>
		internal decimal UnitPriceMoney { get; set; }

		/// <summary>
		///     Gets the permission required.
		/// </summary>
		[JsonProperty(Order = 4)]
		public string PermissionRequired { get; set; }

		/// <summary>
		///		Gets the required items for purchase.
		/// </summary>
		[Browsable(false)]
		[JsonProperty(Order = 5)]
		public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();
		
		public ShopItem()
		{
		}

		public ShopItem(ShopItem other)
		{
			ItemName = other.ItemName;
			StackSize = other.StackSize;
			PrefixId = other.PrefixId;
			UnitPrice = other.UnitPrice;
			PermissionRequired = other.PermissionRequired;
			RequiredItems = other.RequiredItems.Select(r => new RequiredItem(r)).ToList();
		}

		object ICloneable.Clone()
		{
			return new ShopItem(this);
		}

		public override string ToString()
		{
			if( string.IsNullOrWhiteSpace(ItemName) )
				return $"<No Item> @ {UnitPrice}";
			else
				return $"{ItemName} @ {UnitPrice}";
		}
	}
}
