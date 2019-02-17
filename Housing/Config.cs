using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Corruption.PluginSupport;

namespace Housing
{
    /// <summary>
    ///     Represents a configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config : JsonConfig
    {
		//mirrors defaults set in the main Config.
		private GroupConfig defaultGroupConfig;

        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();
		
		/// <summary>
		///  Gets the <see cref="DatabaseConfig"/>. 
		/// </summary>
		[JsonProperty(Order = 0)]
		public DatabaseConfig Database { get; private set; } = new DatabaseConfig("sqlite", $"uri=file://housing/db.sqlite,Version=3");

		/// <summary>
		///     Gets a value indicating whether houses require an admin region to build on.
		/// </summary>
		[JsonProperty(Order = 1)]
		public bool RequireAdminRegions { get; private set; }

		[Obsolete]
		[JsonProperty(Order = 2)]
		public string CurrencyType { get; private set; }

		/// <summary>
		///     Gets the purchase rate.
		/// </summary>
		[JsonProperty(Order = 3)]
		public double PurchaseRate { get; private set; } = 100.0;

		/// <summary>
		///     Gets the tax rate.
		/// </summary>
		[JsonProperty(Order = 4)]
		public double TaxRate { get; private set; } = 1.0;

		/// <summary>
		///     Gets the store tax rate.
		/// </summary>
		[JsonProperty(Order = 5)]
		public double StoreTaxRate { get; private set; } = 10.0;

		/// <summary>
		///     Gets the tax period.
		/// </summary>
		[JsonProperty(Order = 6)]
		public TimeSpan TaxPeriod { get; private set; } = TimeSpan.FromHours(1.0);

		/// <summary>
		///     Gets the maximum debt allowed on a house.
		/// </summary>
		[JsonProperty(Order = 7)]
		public long MaxDebtAllowed { get; private set; } = 10000;

		/// <summary>
		///     Gets a value indicating whether to allow offline shops.
		/// </summary>
		[JsonProperty(Order = 8)]
        public bool AllowOfflineShops { get; private set; }
        
        /// <summary>
        ///     Gets the sales tax rate.
        /// </summary>
        [JsonProperty(Order = 9)]
        public double SalesTaxRate { get; private set; } = 0.07;
        
        /// <summary>
        /// Gets the minimum house size.
        /// </summary>
        [JsonProperty(Order = 10)]
        public int MinHouseSize { get; private set; } = 1000;

        /// <summary>
        /// Gets the maximum house size.
        /// </summary>
        [JsonProperty(Order = 11)]
        public int MaxHouseSize { get; private set; } = 10000;

        /// <summary>
        /// Gets the maximum number of houses.
        /// </summary>
        [JsonProperty(Order = 12)]
        public int MaxHouses { get; private set; } = 10;

        /// <summary>
        /// Gets the minimum shop size.
        /// </summary>
        [JsonProperty(Order = 13)]
        public int MinShopSize { get; private set; } = 1000;

        /// <summary>
        /// Gets the maximum shop size.
        /// </summary>
        [JsonProperty(Order = 14)]
        public int MaxShopSize { get; private set; } = 10000;
		
		/// <summary>
		/// Gets whether the Taxation service is enabled.
		/// </summary>
		[JsonProperty(Order = 15)]
		public bool EnableTaxService { get; private set; } = false;
		
		/// <summary>
		/// Gets per group overrides.
		/// </summary>
		[JsonProperty(Order = 16)]
		public Dictionary<string, GroupConfig> GroupOverrides { get; private set; } = new Dictionary<string, GroupConfig>();

		/// <summary>
		/// Returns a GroupConfig for the given group name.
		/// </summary>
		/// <remarks>This will return a GroupConfig containing the set default values, if no group config exists for the groupname.</remarks>
		/// <param name="groupName"></param>
		/// <returns>GroupConfig</returns>
		public GroupConfig GetGroupConfig(string groupName)
		{
			 GroupConfig cfg = null;

			 if(!GroupOverrides.TryGetValue(groupName, out cfg))
			 {
				 if(defaultGroupConfig==null)
				 {
					defaultGroupConfig = new GroupConfig();
					defaultGroupConfig.CopyDefaults(this);
				 }

				 cfg = defaultGroupConfig;
			 }

			 return cfg;
		}

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			if (Database == null)
				result.Errors.Add(new ValidationError($"Database is null."));

			if (MaxShopSize < MinShopSize)
				result.Warnings.Add(new ValidationWarning($"MaxShopSize({MaxShopSize}) is less than MinShopSize({MinShopSize})."));

			if (MaxHouseSize < MinHouseSize)
				result.Warnings.Add(new ValidationWarning($"MaxHouseSize({MaxHouseSize}) is less than MinHouseSize({MinHouseSize})."));

			if (MaxHouses < 1)
				result.Warnings.Add(new ValidationWarning($"MaxHouses is less than 1. No houses are allowed!"));

			return result;
		}
	}
}
