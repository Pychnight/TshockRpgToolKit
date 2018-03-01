using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class BankAccount
	{
		object locker = new object();

		public string OwnerName { get; private set; }
		public string CurrencyType { get; private set; }
		public decimal Balance { get; private set; }

		internal BankAccount(string ownerName, string currencyType, decimal startingFunds)
		{
			OwnerName = ownerName;
			CurrencyType = currencyType;
			Balance = startingFunds;
		}

		public void Set(decimal amount)
		{
			var plugin = BankingPlugin.Instance;
			decimal newBalance, previousBalance;

			lock( locker )
			{
				previousBalance = Balance;
				Balance = amount;
				newBalance = Balance;
				plugin.BankAccountManager.Database.Update(this);
			}

			plugin.InvokeBalanceChanged(this, newBalance, previousBalance);
		}

		public void Deposit(decimal amount)
		{
			var plugin = BankingPlugin.Instance;
			decimal newBalance, previousBalance;

			if( amount < 0 )
				return;

			lock(locker)
			{
				previousBalance = Balance;
				Balance += amount;
				newBalance = Balance;
				plugin.BankAccountManager.Database.Update(this);
			}

			plugin.InvokeBalanceChanged(this, newBalance, previousBalance);
		}

		public bool TryWithdraw(decimal amount, bool allowOverdraw = false)
		{
			var plugin = BankingPlugin.Instance;
			decimal newBalance, previousBalance;

			if( amount < 0 )
				return false;

			lock(locker)
			{
				if( Balance - amount < 0 && !allowOverdraw )
				{
					return false;
				}

				previousBalance = Balance;
				Balance -= amount;
				newBalance = Balance;
				plugin.BankAccountManager.Database.Update(this);
			}

			plugin.InvokeBalanceChanged(this, newBalance, previousBalance);

			return true;
		}

		public bool TryTransferTo(BankAccount other, decimal amount)
		{
			if( other == null )
				return false;

			if(TryWithdraw(amount))
			{
				other.Deposit(amount);
				return true;
			}

			return false;
		}
	}
}
