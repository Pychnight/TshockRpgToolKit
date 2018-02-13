using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Banking
{
	/// <summary>
	/// Maintains track of which players have hit an NPC, and how many times.
	/// </summary>
	public class NpcStrikeTracker
	{
		Dictionary<int, PlayerStrikeInfo> npcStrikes;

		public event EventHandler<StruckNpcKilledEventArgs> StruckNpcKilled;

		public NpcStrikeTracker()
		{
			npcStrikes = new Dictionary<int, PlayerStrikeInfo>();
		}

		public void OnNpcStrike(Player player, NPC npc)
		{
			var playerIndex = player.whoAmI;
			var npcIndex = npc.whoAmI;
			PlayerStrikeInfo playerStrikes = null;

			if( !npcStrikes.TryGetValue(npcIndex, out playerStrikes) )
			{
				playerStrikes = new PlayerStrikeInfo();
				playerStrikes.OriginalNpcType = npc.type;//used to help determine despawns.
				npcStrikes.Add(npcIndex, playerStrikes);
			}

			playerStrikes.AddStrike(playerIndex);
		}

		public void OnNpcKilled(NPC npc)
		{
			var npcIndex = npc.whoAmI;

			if( npcStrikes.TryGetValue(npcIndex, out var strikes) )
			{
				npcStrikes.Remove(npcIndex);
				if(StruckNpcKilled!=null)
				{
					var args = new StruckNpcKilledEventArgs(npc,strikes);
					StruckNpcKilled(this, args);
				}
			}
		}
		
		public void OnGameUpdate()
		{
			//check for any npcs that may have despawned.
			removeDespawnedNpcs();
		}

		private void removeDespawnedNpcs()
		{
			var kvps = npcStrikes.ToArray();

			foreach( var kvp in kvps )
			{
				var npcIndex = kvp.Key;

				if( Main.npc[npcIndex]?.active != true )
				{
					Debug.Print($"Npc #{npcIndex} despawned. Removing from strike tracking.");
					npcStrikes.Remove(npcIndex);
				}
			}
		}
	}
}
