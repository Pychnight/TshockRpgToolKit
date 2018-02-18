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
			lock( locker )
			{
				Balance = amount;
				BankingPlugin.Instance.BankAccountManager.Database.Update(this);
			}
		}

		public void Deposit(decimal amount)
		{
			if( amount < 0 )
				return;

			lock(locker)
			{
				Balance += amount;
				BankingPlugin.Instance.BankAccountManager.Database.Update(this);
			}
		}

		public bool TryWithdraw(decimal amount)
		{
			if( amount < 0 )
				return false;

			lock(locker)
			{
				if( Balance - amount < 0 )
				{
					return false;
				}

				Balance -= amount;
				BankingPlugin.Instance.BankAccountManager.Database.Update(this);

				return true;
			}
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
