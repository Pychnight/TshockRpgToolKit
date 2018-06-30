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
	public class ShopCommand : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New ShopCommand";

		[JsonProperty(Order = 0)]
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
		///     Gets the command.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string Command { get; set; }

		///// <summary>
		/////     Gets the name.
		///// </summary>

		//public string Name { get; private set; }

		/// <summary>
		///     Gets the stack size. A value of -1 indicates unlimited.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int StackSize { get; set; } = 1;

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
				//	Debug.Print($"Failed to parse UnitPrice for NpcShop Command '{Name}.' Setting to 1.");
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
		//internal decimal UnitPriceMoney { get; private set; }

		/// <summary>
		///     Gets the permission required.
		/// </summary>
		//[JsonProperty(Order = 4)]
		//public string PermissionRequired { get; private set; }

		/// <summary>
		///		Gets the required items for purchase.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<RequiredItem> RequiredItems { get; private set; } = new List<RequiredItem>();
				
		public ShopCommand()
		{
		}

		public ShopCommand(ShopCommand other)
		{
			Name = other.Name;
			Command = other.Command;
			StackSize = other.StackSize;
			UnitPrice = other.UnitPrice;
			RequiredItems = other.RequiredItems.Select(r => new RequiredItem(r)).ToList();
		}

		object ICloneable.Clone()
		{
			return new ShopCommand(this);
		}

		public override string ToString()
		{
			if( string.IsNullOrWhiteSpace(Name) )
				return $"Unamed ShopCommand @ {UnitPrice}";
			else
				return $"{Name} @ {UnitPrice}";
		}
	}
}
