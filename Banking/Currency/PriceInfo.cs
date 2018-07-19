using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Currency
{
	/// <summary>
	/// Helper class to minimize parsing and duplicated code by caching common pricing information. 
	/// </summary>
	public class PriceInfo
	{
		/// <summary>
		/// Gets the current price string, if any is set. This returns an empty string, if IsValid = false.
		/// </summary>
		public string Price { get; private set; } = "";

		/// <summary>
		/// Gets the generic unit value.
		/// </summary>
		public decimal Value { get; private set; }

		/// <summary>
		/// Gets the Currency used for Price, if any is set.
		/// </summary>
		public CurrencyDefinition Currency { get; private set; }

		/// <summary>
		/// Gets if this PriceInfo is in a usuable state. Dependant on whether Price is valid.
		/// </summary>
		public bool IsValid => Currency != null;

		public PriceInfo()
		{
		}

		/// <summary>
		/// Constructs a PriceInfo and calls the SetPrice() method.
		/// </summary>
		/// <param name="priceString">A string containing a price in value-quadrant format.</param>
		/// <param name="foldPrice">If true, the Price string will be processed further, and reformatted to use the largest quadrants possible.</param>
		public PriceInfo(string priceString, bool foldPrice = true)
		{
			SetPrice(priceString, foldPrice);
		}

		/// <summary>
		/// Updates all properties based on the price string. If the price string can not be parsed, the PriceInfo enters the invalid state,
		/// and all properties are set to default values.
		/// </summary>
		/// <param name="priceString">A string containing a price in value-quadrant format.</param>
		/// <param name="foldPrice">If true, the Price string will be processed further, and reformatted to use the largest quadrants possible.</param>
		/// <returns>True if succeeded, false if the PriceInfo is invalid.</returns>
		public bool SetPrice(string priceString, bool foldPrice = true)
		{
			if(!BankingPlugin.Instance.Bank.CurrencyManager.TryFindCurrencyFromString(priceString, out var currency))
			{
				Reset();
				return false;
			}

			if(!currency.GetCurrencyConverter().TryParse(priceString, out var value))
			{
				Reset();
				return false;
			}

			if(foldPrice)
				Price = currency.GetCurrencyConverter().ToString(value);	
			else
				Price = priceString;
			
			Value = value;
			Currency = currency;

			return true;
		}

		/// <summary>
		/// Resets all properties to default values, and marks the PriceInfo as invalid.
		/// </summary>
		public void Reset()
		{
			Price = "";
			Value = 0m;
			Currency = null;
		}

		public override string ToString()
		{
			return Price;
		}
	}
}
