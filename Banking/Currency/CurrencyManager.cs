using Banking.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
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
		internal Dictionary<string, CurrencyDefinition> Definitions;

		public CurrencyDefinition this[string name]
		{
			get
			{
				Definitions.TryGetValue(name, out var result);
				return result;
			}
		}

		internal CurrencyManager(IEnumerable<CurrencyDefinition> currencies)
		{
			Definitions = new Dictionary<string, CurrencyDefinition>();

			foreach(var cur in currencies)
			{
				//we do this to avoid string parsing on every look up
				//foreach(var kvp in cur.Rewards)
				//	kvp.Value.PreParseValues(cur);
								
				Definitions.Add(cur.InternalName, cur);
			}
		}

		public IEnumerator<CurrencyDefinition> GetEnumerator()
		{
			foreach(var def in Definitions.Values)
			{
				yield return def;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
