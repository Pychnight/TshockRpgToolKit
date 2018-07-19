using Banking.Configuration;
using Corruption.PluginSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// A central collection of loaded Currency's.
	/// </summary>
	public class CurrencyManager : IEnumerable<CurrencyDefinition>
	{
		List<CurrencyDefinition> items;
		Dictionary<string, CurrencyDefinition> CurrencyByName;
		Dictionary<string, CurrencyDefinition> CurrencyByQuadName;

		public int Count => items.Count;
		public CurrencyDefinition this[int id] => GetCurrencyById(id);
		public CurrencyDefinition this[string name] => GetCurrencyByName(name);
		
		internal CurrencyManager(string currencyDirectory) : this( CurrencyDefinition.LoadCurrencys(currencyDirectory))
		{
		}

		internal CurrencyManager(IEnumerable<CurrencyDefinition> currencies)
		{
			var count = currencies.Count();
			var nextId = 0;

			items = new List<CurrencyDefinition>(count);
			CurrencyByName = new Dictionary<string, CurrencyDefinition>(count);
			CurrencyByQuadName = new Dictionary<string, CurrencyDefinition>(count);

			foreach(var currency in currencies)
			{
				currency.OnInitialize(nextId++);

				//map quadrant abbreviations to currencies.
				foreach(var name in currency.NamesToQuadrants.Keys)
				{
					if(CurrencyByQuadName.ContainsKey(name))
					{
						BankingPlugin.Instance.LogPrint($"Quadrant name '{name}' in Currency '{currency.InternalName}' " +
														"will take precedence over another Currency using the same name.",
														TraceLevel.Warning);
					}

					CurrencyByQuadName[name] = currency;	
				}
				
				items.Add(currency);
				CurrencyByName.Add(currency.InternalName, currency);
			}
		}

		//public CurrencyDefinition GetDefaultCurrency()
		//{
		//	return items[0];
		//}

		public CurrencyDefinition GetCurrencyById(int id)
		{
			return items[id];
		}

		public CurrencyDefinition GetCurrencyByName(string name)
		{
			CurrencyByName.TryGetValue(name, out var result);
			return result;
		}

		public CurrencyDefinition GetCurrencyByQuadName(string quadName)
		{
			CurrencyByQuadName.TryGetValue(quadName, out var result);
			return result;
		}

		/// <summary>
		/// Attempts to find the Currency represented by the value string.
		/// </summary>
		/// <param name="value">A string containing a valid currency amount.</param>
		/// <param name="currency">The Currency, if valid. Null otherwise.</param>
		/// <returns>True or false if a Currency was found.</returns>
		public bool TryFindCurrencyFromString(string value, out CurrencyDefinition currency)
		{
			currency = null;
			
			var quadNames = CurrencyConverter.ParseQuadrantNames(value);
			if( quadNames.Count < 1 )
				return false;
			
			//try to find if the first suffix/quad name is valid
			var firstQuad = quadNames.First();
			var selectedCurrency = GetCurrencyByQuadName(firstQuad);
			if( selectedCurrency == null )
				return false;
			
			//ensure all quads are valid, and lead to the same currency
			foreach( var quadName in quadNames )
			{
				var cur = GetCurrencyByQuadName(quadName);
				if( cur != selectedCurrency )
					return false;
			}

			currency = selectedCurrency;
			return true;
		}

		public IEnumerator<CurrencyDefinition> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
