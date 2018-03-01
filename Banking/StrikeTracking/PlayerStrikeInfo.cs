using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Records the number of times each Player has struck a given NPC.
	/// </summary>
	public class PlayerStrikeInfo : Dictionary<string, StrikeInfo>
	{
		//"hidden" member, used to help determine when npc's despawn...
		internal int OriginalNpcType { get; set; }

		public void AddStrike(string playerName, int damage)
		{
			if( !ContainsKey(playerName) )
				Add(playerName, new StrikeInfo());

			this[playerName].AddStrike(damage);
		}
	}
}
