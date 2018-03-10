using System;
using Terraria;

namespace Banking
{
	public class StruckNpcKilledEventArgs : EventArgs
	{
		//public NPC Npc { get; private set; }
		public int NpcType { get; private set; }
		public float NpcValue { get; private set; }
		public string NpcGivenOrTypeName { get; private set; }
		public bool NpcSpawnedFromStatue { get; private set; }
		public PlayerStrikeInfo PlayerStrikeInfo { get; private set; }

		internal StruckNpcKilledEventArgs(NPC npc, PlayerStrikeInfo playerStrikeInfo)
		{
			NpcValue = npc.value;
			NpcType = npc.type;
			NpcGivenOrTypeName = npc.GivenOrTypeName;
			NpcSpawnedFromStatue = npc.SpawnedFromStatue;
			PlayerStrikeInfo = playerStrikeInfo;
		}
	}
}