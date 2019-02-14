using Banking;
using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Wolfje.Plugins.SEconomy;

namespace NpcShops.Shops
{
	/// <summary>
	///		Common base class for shop sellable items.
	/// </summary>
	public abstract class ShopProduct
	{
		public bool IsValid { get; protected set; }

		/// <summary>
		///     Gets or sets the stack size. A value of -1 indicates unlimited.
		/// </summary>
		public int StackSize { get; set; }

		/// <summary>
		/// Gets the unit price string.
		/// </summary>
		public string UnitPriceString { get; set; }
		
		/// <summary>
		///     Gets the unit price.
		/// </summary>
		public decimal UnitPrice { get; set; }

		/// <summary>
		/// Gets the Currency used for this ShopProduct.
		/// </summary>
		public CurrencyDefinition Currency { get; private set; }

		/// <summary>
		///		Gets the items required for purchase.
		/// </summary>
		public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();//just a placeholder for now

		/// <summary>
		///    Restocks the shop product.
		/// </summary>
		public abstract void Restock();

		/// <summary>
		/// Helper method to automatically initialize a ShopProduct's UnitPrice, and Currency from the UnitPriceString.
		/// </summary>
		/// <param name="unitPriceString"></param>
		/// <returns></returns>
		protected bool TryResolvePricing(string unitPriceString)
		{
			try
			{
				var mgr = BankingPlugin.Instance.Bank.CurrencyManager;

				if( mgr.TryFindCurrencyFromString(unitPriceString, out var currency))
				{
					if( currency.GetCurrencyConverter().TryParse(unitPriceString, out var parsedPrice))
					{
						UnitPrice = parsedPrice;
						Currency = currency;
						return true;
					}
				}
			}
			catch(Exception ex)
			{
				NpcShopsPlugin.Instance.LogPrint(ex.Message, TraceLevel.Error);
			}
						
			return false;
		}
	}
}
