using Banking.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class CurrencyManager
	{
		//public static CurrencyManager Instance { get; private set; }

		internal Dictionary<string, CurrencyDefinition> Definitions;

		public CurrencyManager(IEnumerable<CurrencyDefinition> currencies)
		{
			Definitions = new Dictionary<string, CurrencyDefinition>();

			foreach(var cur in currencies)
				Definitions.Add(cur.InternalName, cur);
		}
	}
}
