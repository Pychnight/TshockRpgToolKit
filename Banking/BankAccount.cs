﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class BankAccount
	{
		static object updateLocker = new object();
		//dictionary of accounts that been modified, and need to be persisted to the db
		internal static ConcurrentDictionary<int, BankAccount> AccountsToUpdate { get; set; } = new ConcurrentDictionary<int, BankAccount>();

		object locker = new object();

		//used only as a key within the update set, no relevance to db, or even between sessions.
		internal int InternalId { get; private set; }

		/// <summary>
		/// Gets the account Name. This is the same as the Currency type this account backs.  
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the Player name of this accounts owner.
		/// </summary>
		public string OwnerName { get; private set; }

		/// <summary>
		/// Gets the total funds available in this account, in generic units.
		/// </summary>
		public decimal Balance { get; internal set; }

		internal BankAccount(string ownerName, string name, decimal startingFunds)
		{
			OwnerName = ownerName;
			Name = name;
			Balance = startingFunds;
		}

		internal static void UpdateAccounts()
		{
			var bank = BankingPlugin.Instance.Bank;
			var accounts = AccountsToUpdate.Select( kvp => kvp.Value ).ToList();
			
			AccountsToUpdate.Clear();//we need better sync here, stuff can happen between above line and the clear().

			Task.Run(() =>
			{
				Debug.Print($"Updating {accounts.Count} BankAccounts.");
				bank.Database.Update(accounts);
			});
		}

		private void markForUpdate()
		{
			AccountsToUpdate.TryAdd(InternalId, this);
		}

		/// <summary>
		/// Sets the Balance to a specified amount. Does not raise the BalanceChanged event.
		/// </summary>
		/// <param name="amount">New balance.</param>
		public void Set(decimal amount)
		{
			var bank = BankingPlugin.Instance.Bank;
			//decimal newBalance, previousBalance;

			lock( locker )
			{
				//previousBalance = Balance;
				Balance = amount;
				//newBalance = Balance;
				//bank.Database.Update(this);
				markForUpdate();
			}

			//bank.InvokeBalanceChanged(this, newBalance, previousBalance);
		}

		/// <summary>
		/// Deposits the specified amount into the BankAccount.
		/// </summary>
		/// <param name="amount">Amount to add.</param>
		public void Deposit(decimal amount)
		{
			var bank = BankingPlugin.Instance.Bank;
			decimal newBalance, previousBalance;

			if( amount < 0 )
				return;

			lock(locker)
			{
				previousBalance = Balance;
				Balance += amount;
				newBalance = Balance;
				//bank.Database.Update(this);
				markForUpdate();
			}

			bank.InvokeBalanceChanged(this, newBalance, previousBalance);
		}

		/// <summary>
		/// Tries to withdraw a specified amount from the BankAccount's Balance.
		/// </summary>
		/// <param name="amount">Amount to withdraw.</param>
		/// <param name="allowOverdraw">True if the account can go negative.</param>
		/// <returns>True if transaction succeeded.</returns>
		public bool TryWithdraw(decimal amount, WithdrawalMode withdrawalMode = WithdrawalMode.RequireFullBalance)// bool allowOverdraw = false)
		{
			var bank = BankingPlugin.Instance.Bank;
			decimal newBalance, previousBalance;

			if( amount < 0 )
			{
				amount = Math.Abs(amount);
			}

			lock(locker)
			{
				if( ( Balance - amount ) < 0 )
				{
					if( withdrawalMode == WithdrawalMode.RequireFullBalance)
						return false;
					
					if( withdrawalMode == WithdrawalMode.StopAtZero)
						amount = Balance;
				}
				
				previousBalance = Balance;
				Balance -= amount;
				newBalance = Balance;
				//bank.Database.Update(this);
				markForUpdate();
			}

			bank.InvokeBalanceChanged(this, newBalance, previousBalance);

			return true;
		}

		/// <summary>
		/// Attempts to transfer funds from one BankAccount to another.
		/// </summary>
		/// <param name="other">Destination BankAccount.</param>
		/// <param name="amount">Amount to transfer.</param>
		/// <param name="allowOverdraw">True if the transaction can leave the source account negative.</param>
		/// <returns>True if the transaction succeeded.</returns>
		public bool TryTransferTo(BankAccount other, decimal amount, WithdrawalMode withdrawalMode = WithdrawalMode.RequireFullBalance)
		{
			if( other == null )
				return false;

			if(TryWithdraw(amount,withdrawalMode))
			{
				other.Deposit(amount);
				return true;
			}

			return false;
		}
	}
}
