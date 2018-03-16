﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class StrikeInfo
	{
		//public DateTime LastStrikeTime { get; private set; }
		public int Strikes { get; private set; }
		public int Damage { get; private set; }
		public string ItemName { get; private set; } //since were only concerned with what weapon was equipped that killed the npc, we can overwrite this.

		public void AddStrike(int damage, string itemName)
		{
			//LastStrikeTime = DateTime.Now;
			Strikes += 1;
			Damage += damage;
			ItemName = itemName;
		}
	}
}
