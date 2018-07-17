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
