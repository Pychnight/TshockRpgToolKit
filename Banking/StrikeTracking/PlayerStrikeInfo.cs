using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class PlayerStrikeInfo : Dictionary<int, StrikeInfo>
	{
		//"hidden" member, used to help determine when npc's despawn...
		internal int OriginalNpcType { get; set; }

		public void AddStrike(int playerIndex)
		{
			if( !ContainsKey(playerIndex) )
				Add(playerIndex, new StrikeInfo());

			this[playerIndex].AddStrike();
		}
	}
}
