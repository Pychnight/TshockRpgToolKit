using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		///     Gets the closed message.
		/// </summary>
		[JsonProperty(Order = 9)]
		public string ClosedMessage { get; private set; }

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
		///		Gets the town npc types that this shop overrides.
		/// </summary>
		[JsonProperty(Order = 8)]
		public List<int> OverrideNpcTypes { get; private set; } = new List<int>();

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static NpcShopDefinition TryLoadFromFile(string filePath)
		{
			NpcShopDefinition result = null;

			try
			{
				var txt = File.ReadAllText(filePath);
				result = JsonConvert.DeserializeObject<NpcShopDefinition>(txt);

				return result;
			}
			catch( JsonReaderException jrex )
			{
				NpcShopsPlugin.Instance.LogPrint($"A json error occured while trying to load NpcShop {filePath}.", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint(jrex.Message, TraceLevel.Error);
			}
			catch( Exception ex )
			{
				NpcShopsPlugin.Instance.LogPrint($"An error occured while trying to load NpcShop {filePath}.", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint(ex.Message, TraceLevel.Error);
			}

			NpcShopsPlugin.Instance.LogPrint("Shop disabled.", TraceLevel.Error);

			return result;
		}
    }
}
