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

		public string Owner { get; private set; }
		public decimal Balance { get; private set; }

		public BankAccount(string owner, decimal startingFunds)
		{
			Owner = owner;
			Balance = startingFunds;
		}

		public void Deposit(decimal amount)
		{
			lock(locker)
			{
				Balance += amount;
			}
		}

		public bool TryWithdraw(decimal amount)
		{
			lock(locker)
			{
				if( Balance - amount < 0 )
				{
					return false;
				}

				Balance -= amount;

				return true;
			}
		}

		public bool TryTransferTo(BankAccount other, decimal amount)
		{
			if(TryWithdraw(amount))
			{
				other.Deposit(amount);
				return true;
			}

			return false;
		}
	}
}
