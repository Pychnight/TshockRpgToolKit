﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RpgToolsEditor.Controls;
using Newtonsoft.Json;

namespace RpgToolsEditor.Models.NpcShops
{
	/// <summary>
	///     Represents an NPC shop definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NpcShopDefinition : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New NpcShop";

		[Browsable(false)]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Name)));
			}
		}
		
		/// <summary>
		///     Gets the closing time.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 2)]
		public string ClosingTime { get; set; }

		/// <summary>
		///     Gets the message.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 3)]
		public string Message { get; set; }

		/// <summary>
		///     Gets the closed message.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 9)]
		public string ClosedMessage { get; set; }

		/// <summary>
		///     Gets the opening time.
		/// </summary>
		[Category("Basic Properties")]
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
		public List<int> OverrideNpcTypes { get; set; } = new List<int>();

		/// <summary>
		///     Gets the restock time.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 6)]
		public TimeSpan RestockTime { get; set; } = TimeSpan.FromMinutes(1);

		/// <summary>
		///     Gets the sales tax rate.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 7)]
		public double SalesTaxRate { get; set; } = 0.07;

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 5)]
		public List<ShopCommandDefinition> ShopCommands { get; set; } = new List<ShopCommandDefinition>();

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 4)]
		public List<ShopItemDefinition> ShopItems { get; set; } = new List<ShopItemDefinition>();

		public NpcShopDefinition()
		{
		}

		public NpcShopDefinition(NpcShopDefinition other)
		{
			Name = other.Name;
			ClosingTime = other.ClosingTime;
			Message = other.Message;
			ClosedMessage = other.ClosedMessage;
			OpeningTime = other.OpeningTime;
			RegionName = other.RegionName;
			OverrideNpcTypes = new List<int>(other.OverrideNpcTypes);
			RestockTime = other.RestockTime;
			SalesTaxRate = other.SalesTaxRate;
			ShopCommands = other.ShopCommands.Select(s => new ShopCommandDefinition(s)).ToList();
			ShopItems = other.ShopItems.Select(s => new ShopItemDefinition(s)).ToList();
		}
	}
}
