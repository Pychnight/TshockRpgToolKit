using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Housing
{
	[JsonObject(MemberSerialization.OptIn)]
	public class GroupConfig
	{
		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(Order = 0)]
		public double PurchaseRate { get; private set; } = 30.0f;

		/// <summary>
		/// Gets the group tax rate.
		/// </summary>
		[JsonProperty(Order = 1)]
		public double TaxRate { get; private set; } = 2.0f;

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(Order = 2)]
		public double StoreTaxRate { get; private set; } = 10.0f;

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(Order = 3)]
		public TimeSpan TaxPeriod { get; private set; } = new TimeSpan(0, 0, 30, 0, 0);

		/// <summary>
		///	
		/// </summary>
		[JsonProperty(Order = 4)]
		public long MaxDebtAllowed { get; private set; } = 10000;

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(Order = 5)]
		public bool AllowOfflineShops { get; private set; } = false;

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(Order = 6)]
		public double SalesTaxRate { get; private set; } = 0.07f;

		/// <summary>
		/// Gets the minimum house size.
		/// </summary>
		[JsonProperty(Order = 7)]
		public int MinHouseSize { get; private set; } = 1000;

		/// <summary>
		/// Gets the maximum house size.
		/// </summary>
		[JsonProperty(Order = 8)]
		public int MaxHouseSize { get; private set; } = 10000;

		/// <summary>
		/// Gets the maximum number of houses.
		/// </summary>
		[JsonProperty(Order = 9)]
		public int MaxHouses { get; private set; } = 10;

		/// <summary>
		/// Gets the minimum shop size.
		/// </summary>
		[JsonProperty(Order = 10)]
		public int MinShopSize { get; private set; } = 1000;

		/// <summary>
		/// Gets the maximum shop size.
		/// </summary>
		[JsonProperty(Order = 11)]
		public int MaxShopSize { get; private set; } = 10000;

		internal void CopyDefaults(Config config)
		{
			//update default cfg.
			PurchaseRate = config.PurchaseRate;
			TaxRate = config.TaxRate;
			StoreTaxRate = config.StoreTaxRate;
			TaxPeriod = config.TaxPeriod;
			MaxDebtAllowed = config.MaxDebtAllowed;
			AllowOfflineShops = config.AllowOfflineShops;
			SalesTaxRate = config.SalesTaxRate;
			MinHouseSize = config.MinHouseSize;
			MaxHouseSize = config.MaxHouseSize;
			MaxHouses = config.MaxHouses;
			MinShopSize = config.MinShopSize;
			MaxShopSize = config.MaxShopSize;
		}
	}
}
