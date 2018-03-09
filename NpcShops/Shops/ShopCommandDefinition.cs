using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace NpcShops.Shops
{
    /// <summary>
    ///     Represents a shop command definition.
    /// </summary>
    public sealed class ShopCommandDefinition
    {
        /// <summary>
        ///     Gets the command.
        /// </summary>
        [JsonProperty(Order = 1)]
        public string Command { get; private set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [JsonProperty(Order = 0)]
        public string Name { get; private set; }
		
        /// <summary>
        ///     Gets the stack size. A value of -1 indicates unlimited.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int StackSize { get; private set; }

		string unitPrice;

		/// <summary>
		///     Gets the unit price.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string UnitPrice
		{
			get => unitPrice;
			private set
			{
				unitPrice = value;

				if( !NpcShopsPlugin.Instance.Currency.GetCurrencyConverter().TryParse(value, out var result) )
				{
					Debug.Print($"Failed to parse UnitPrice for NpcShop Command '{Name}.' Setting to 1.");
					UnitPriceMoney = 1;
				}
				else
				{
					UnitPriceMoney = result;
				}
			}
		}

		/// <summary>
		///		Gets the numeric value of the UnitPrice string.
		/// </summary>
		internal decimal UnitPriceMoney { get; private set; }

		/// <summary>
		///     Gets the permission required.
		/// </summary>
		//[JsonProperty(Order = 4)]
		//public string PermissionRequired { get; private set; }

		/// <summary>
		///		Gets the required items for purchase.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<RequiredItemDefinition> RequiredItems { get; private set; } = new List<RequiredItemDefinition>();
	}
}
