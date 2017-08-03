using System;
using Newtonsoft.Json;

namespace Housing
{
    /// <summary>
    ///     Represents a configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

        /// <summary>
        ///     Gets a value indicating whether to allow offline shops.
        /// </summary>
        [JsonProperty(Order = 6)]
        public bool AllowOfflineShops { get; private set; }

        /// <summary>
        ///     Gets the maximum debt allowed on a house.
        /// </summary>
        [JsonProperty(Order = 5)]
        public long MaxDebtAllowed { get; private set; } = 10000;

        /// <summary>
        ///     Gets the purchase rate.
        /// </summary>
        [JsonProperty(Order = 1)]
        public double PurchaseRate { get; private set; } = 100.0;

        /// <summary>
        ///     Gets a value indicating whether houses require an admin region to build on.
        /// </summary>
        [JsonProperty(Order = 0)]
        public bool RequireAdminRegions { get; private set; }

        /// <summary>
        ///     Gets the sales tax rate.
        /// </summary>
        [JsonProperty(Order = 7)]
        public double SalesTaxRate { get; private set; } = 0.07;

        /// <summary>
        ///     Gets the store tax rate.
        /// </summary>
        [JsonProperty(Order = 3)]
        public double StoreTaxRate { get; private set; } = 10.0;

        /// <summary>
        ///     Gets the tax period.
        /// </summary>
        [JsonProperty(Order = 4)]
        public TimeSpan TaxPeriod { get; private set; } = TimeSpan.FromHours(1.0);

        /// <summary>
        ///     Gets the tax rate.
        /// </summary>
        [JsonProperty(Order = 2)]
        public double TaxRate { get; private set; } = 1.0;
    }
}
