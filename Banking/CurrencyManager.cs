using Banking.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class CurrencyManager : IEnumerable<CurrencyDefinition>
	{
		//public static CurrencyManager Instance { get; private set; }

		internal Dictionary<string, CurrencyDefinition> Definitions;

		public CurrencyDefinition this[string name]
		{
			get
			{
				Definitions.TryGetValue(name, out var result);
				return result;
			}
		}

		public CurrencyManager(IEnumerable<CurrencyDefinition> currencies)
		{
			Definitions = new Dictionary<string, CurrencyDefinition>();

			foreach(var cur in currencies)
				Definitions.Add(cur.InternalName, cur);
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
