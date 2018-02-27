using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public struct StrikeInfo
	{
		//public DateTime LastStrikeTime { get; private set; }
		public int Strikes { get; private set; }

		public void AddStrike()
		{
			//LastStrikeTime = DateTime.Now;
			Strikes += 1;
		}
	}
}
