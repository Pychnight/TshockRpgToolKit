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
		List<CurrencyDefinition> definitions;
		internal Dictionary<string, CurrencyDefinition> DefinitionsByName;

		public int Count => definitions.Count;

		public CurrencyDefinition this[int id] => definitions[id];

		public CurrencyDefinition this[string name]
		{
			get
			{
				DefinitionsByName.TryGetValue(name, out var result);
				return result;
			}
		}

		internal CurrencyManager(IEnumerable<CurrencyDefinition> currencies)
		{
			var count = currencies.Count();
			var nextId = 0;

			definitions = new List<CurrencyDefinition>(count);
			DefinitionsByName = new Dictionary<string, CurrencyDefinition>(count);

			foreach(var cur in currencies)
			{
				//we do this to avoid string parsing on every look up
				//foreach(var kvp in cur.Rewards)
				//	kvp.Value.PreParseValues(cur);
				cur.Id = nextId++;
				cur.UpdateInfoString();
				
				definitions.Add(cur);
				DefinitionsByName.Add(cur.InternalName, cur);
			}
		}

		public IEnumerator<CurrencyDefinition> GetEnumerator()
		{
			foreach(var def in DefinitionsByName.Values)
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
