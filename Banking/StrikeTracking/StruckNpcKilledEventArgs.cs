using System;
using Terraria;

namespace Banking
{
	public class StruckNpcKilledEventArgs : EventArgs
	{
		//public NPC Npc { get; private set; }
		public int NpcType { get; private set; }
		/// <summary>
		/// Gets the hit points of the NPC recorded at spawn time.
		/// </summary>
		public int NpcHitPoints { get; private set; }
		//public float NpcValue { get; private set; }
		public string NpcGivenOrTypeName { get; private set; }
		public bool NpcSpawnedFromStatue { get; private set; }
		public PlayerStrikeInfo PlayerStrikeInfo { get; private set; }

		internal StruckNpcKilledEventArgs(NPC npc, int npcHitPoints, PlayerStrikeInfo playerStrikeInfo)
		{
			//Npc = npc;
			//NpcValue = npc.value;
			NpcHitPoints = npcHitPoints;
			NpcType = npc.type;
			NpcGivenOrTypeName = npc.GivenOrTypeName;
			NpcSpawnedFromStatue = npc.SpawnedFromStatue;
			PlayerStrikeInfo = playerStrikeInfo;
		}
	}
}