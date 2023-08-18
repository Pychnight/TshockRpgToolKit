using System;
using System.Collections.Generic;

namespace Banking
{
	/// <summary>
	/// Records the number of times each Player has struck a given NPC.
	/// </summary>
	public class PlayerStrikeInfo : Dictionary<string, StrikeInfo>
	{
		//"hidden" member, used to help determine when npc's despawn...
		internal int OriginalNpcType { get; set; }

		public void AddStrike(string playerName, int damage, int damageDefended, string itemName)
		{
			if (itemName == null)
				throw new Exception();

			if (!ContainsKey(playerName))
				Add(playerName, new StrikeInfo());

			if (damage < 1)
			{
				//we only add damageDefended if we hurt the npc
				damageDefended = 0;
			}

			this[playerName].AddStrike(damage, damageDefended, itemName);
		}
	}
}
