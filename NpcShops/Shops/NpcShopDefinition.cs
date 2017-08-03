using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NpcShops.Shops
{
    /// <summary>
    ///     Represents an NPC shop definition.
    /// </summary>
    public sealed class NpcShopDefinition
    {
        /// <summary>
        ///     Gets the closing time.
        /// </summary>
        [JsonProperty(Order = 2)]
        public string ClosingTime { get; private set; }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        [JsonProperty(Order = 3)]
        public string Message { get; private set; }

        /// <summary>
        ///     Gets the opening time.
        /// </summary>
        [JsonProperty(Order = 1)]
        public string OpeningTime { get; private set; }

        /// <summary>
        ///     Gets the region name.
        /// </summary>
        [JsonProperty(Order = 0)]
        public string RegionName { get; private set; }

        /// <summary>
        ///     Gets the restock time.
        /// </summary>
        [JsonProperty(Order = 6)]
        public TimeSpan RestockTime { get; private set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     Gets the sales tax rate.
        /// </summary>
        [JsonProperty(Order = 7)]
        public double SalesTaxRate { get; private set; } = 0.07;

        /// <summary>
        ///     Gets the list of shop items.
        /// </summary>
        [JsonProperty(Order = 5)]
        public IList<ShopCommandDefinition> ShopCommands { get; private set; } = new List<ShopCommandDefinition>();

        /// <summary>
        ///     Gets the list of shop items.
        /// </summary>
        [JsonProperty(Order = 4)]
        public IList<ShopItemDefinition> ShopItems { get; private set; } = new List<ShopItemDefinition>();
    }
}
