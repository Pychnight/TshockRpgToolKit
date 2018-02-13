using System;
using Terraria;

namespace Banking
{
	public class StruckNpcKilledEventArgs : EventArgs
	{
		public NPC Npc { get; private set; }
		public PlayerStrikeInfo PlayerStrikeInfo { get; private set; }

		internal StruckNpcKilledEventArgs(NPC npc, PlayerStrikeInfo playerStrikeInfo)
		{
			PlayerStrikeInfo = playerStrikeInfo;
		}
	}
}