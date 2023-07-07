using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RpgToolsEditor.Models.NpcShops
{
	/// <summary>
	///     Represents an NPC shop definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NpcShop : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "NewNpcShop.shop";

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

		[Category("Filesystem")]
		[Description("The file name of this shop within the folder.")]
		public string Filename { get => Name; set => Name = value; }

		/// <summary>
		///     Gets the closing time.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 2)]
		public string ClosingTime { get; set; }

		/// <summary>
		///     Gets the message.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 3)]
		public string Message { get; set; }

		/// <summary>
		///     Gets the closed message.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 9)]
		public string ClosedMessage { get; set; }

		/// <summary>
		///     Gets the opening time.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 1)]
		public string OpeningTime { get; set; }

		/// <summary>
		///     Gets the region name.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 0)]
		public string RegionName { get; set; }

		/// <summary>
		///		Gets the town npc types that this shop overrides.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 8)]
		//[Editor(typeof(StringCollectionEditor), typeof(UITypeEditor))]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
		public List<string> OverrideNpcTypes { get; set; } = new List<string>();

		/// <summary>
		///     Gets the restock time.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 6)]
		public TimeSpan RestockTime { get; set; } = TimeSpan.FromMinutes(1);

		/// <summary>
		///     Gets the sales tax rate.
		/// </summary>
		[Category("Operations")]
		[JsonProperty(Order = 7)]
		public double SalesTaxRate { get; set; } = 0.07;

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		//[Category("Inventory")] // for new editor
		[Browsable(false)]
		[JsonProperty(Order = 5)]
		public List<ShopCommand> ShopCommands { get; set; } = new List<ShopCommand>();

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		//[Category("Inventory")] // for new editor
		[Browsable(false)]
		[JsonProperty(Order = 4)]
		public List<ShopItem> ShopItems { get; set; } = new List<ShopItem>();

		public NpcShop()
		{
		}

		public NpcShop(NpcShop other)
		{
			Name = other.Name;
			ClosingTime = other.ClosingTime;
			Message = other.Message;
			ClosedMessage = other.ClosedMessage;
			OpeningTime = other.OpeningTime;
			RegionName = other.RegionName;
			OverrideNpcTypes = new List<string>(other.OverrideNpcTypes);
			RestockTime = other.RestockTime;
			SalesTaxRate = other.SalesTaxRate;
			ShopCommands = other.ShopCommands.Select(s => new ShopCommand(s)).ToList();
			ShopItems = other.ShopItems.Select(s => new ShopItem(s)).ToList();
		}

		object ICloneable.Clone() => new NpcShop(this);
	}
}
