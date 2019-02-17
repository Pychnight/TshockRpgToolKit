using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Housing.Models
{
	/// <summary>
	/// Represents a Player whom receives monetary compensation for property and financial transactions. 
	/// </summary>
	//This class is used to maintain orthagonality with other model objects in the database. ( used to be just a plain string )
	public class TaxCollector
	{
		public string PlayerName { get; private set; }

		public TaxCollector(string playerName)
		{
			PlayerName = playerName;
		}
	}
}
