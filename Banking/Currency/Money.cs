using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Currency
{
	public struct Money
	{
		internal decimal Value;
		internal short CurrencyIndex;//index of the currency
		internal short CurrencyManagerVersion;//each time the currency manager loads the currencies from the config, its version is incremented.
												//Money tracks this so that we do not reference/index into a new currency.
	}
}
