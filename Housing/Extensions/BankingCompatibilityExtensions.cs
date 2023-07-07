using Banking;
using System;
using TShockAPI;

namespace Housing.Extensions
{
	/// <summary>
	/// Helpers for transition from SEconomy to Banking/Currency.
	/// </summary>
	public static class BankingCompatibilityExtensions
	{
		public static BankAccount GetBankAccount(this BankingPlugin bankingPlugin, TSPlayer player) => bankingPlugin.GetBankAccount(player, Config.Instance.CurrencyType);

		public static BankAccount GetWorldAccount(this BankingPlugin bankingPlugin) => bankingPlugin.GetBankAccount("Server", Config.Instance.CurrencyType);

		public static bool TryParseMoney(this string input, out decimal value)
		{
			CurrencyDefinition currency = null;

			if (BankingPlugin.Instance.TryGetCurrency(Config.Instance.CurrencyType, out currency))
			{
				var converter = currency.GetCurrencyConverter();
				return converter.TryParse(input, out value);
			}

			value = 0m;
			return false;
		}

		[Obsolete]
		public static string ToMoneyString(this decimal value)
		{
			CurrencyDefinition currency = null;

			if (BankingPlugin.Instance.TryGetCurrency(Config.Instance.CurrencyType, out currency))
			{
				var converter = currency.GetCurrencyConverter();
				return converter.ToString(value);
			}
			else
				return value.ToString();
		}
	}
}
