using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Determines how a withdrawal from a BankAccount is handled.
	/// </summary>
	public enum WithdrawalMode
	{
		/// <summary>
		/// The account's Balance must be large enough to satisfy the withdrawal in order to succeed.
		/// </summary>
		RequireFullBalance,
		/// <summary>
		/// The account's Balance is allowed to go negative, if the amounts is greater than the available Balance.
		/// </summary>
		AllowOverdraw,
		/// <summary>
		/// The withdrawal will succeed, but not take more than the available funds within the account.
		/// </summary>
		StopAtZero
	}
}
