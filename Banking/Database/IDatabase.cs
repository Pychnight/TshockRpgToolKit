using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Database
{
	public interface IDatabase
	{
		string ConnectionString { get; set; }

		IEnumerable<BankAccount> Load();
		void Save(IEnumerable<BankAccount> accounts);

		void Create(BankAccount account);
		void Create(IEnumerable<BankAccount> accounts);
		
		void Update(BankAccount account);
		void Update(IEnumerable<BankAccount> accounts);

		void Delete(BankAccount account);
		void Delete(IEnumerable<BankAccount> accounts);
	}
}
