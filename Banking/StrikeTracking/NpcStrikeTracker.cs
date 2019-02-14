using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Banking
{
	/// <summary>
	/// Tracks which players have hit an NPC, and how many times.
	/// </summary>
	public class NpcStrikeTracker
	{
		//Dictionary<int, PlayerStrikeInfo> npcStrikes;
		ConcurrentDictionary<int, PlayerStrikeInfo> npcStrikes;

		public event EventHandler<StruckNpcKilledEventArgs> StruckNpcKilled;

		public NpcStrikeTracker()
		{
			//npcStrikes = new Dictionary<int, PlayerStrikeInfo>();
			npcStrikes = new ConcurrentDictionary<int, PlayerStrikeInfo>();
		}

		public void Clear()
		{
			npcStrikes.Clear();
		}

		public static int CalculateNpcDamage(NPC npc, double damage, bool isCritical)
		{
			//pulled from MarioE's damage calc in LevelingPlugin.OnNetGetData()...
			if( damage < 1.0d )
				return 0;

			var defense = CalculateNpcDefense(npc);

			damage = Main.CalculateDamage((int)damage, defense);
			damage *= isCritical ? 2.0 : 1.0;
			damage *= Math.Max(1.0, npc.takenDamageMultiplier);

			return (int)damage;
		}

		public static int CalculateNpcDefense(NPC npc)
		{
			var defense = npc.defense;

			defense -= npc.ichor ? 20 : 0;
			defense -= npc.betsysCurse ? 40 : 0;
			defense = Math.Max(0, defense);

			return defense;
		}

		public void OnNpcStrike(Player player, NPC npc, int damage, bool isCritical, string itemName )
		{
			//var playerIndex = player.whoAmI;
			var npcIndex = npc.whoAmI;
			PlayerStrikeInfo playerStrikes = null;

			if( !npcStrikes.TryGetValue(npcIndex, out playerStrikes) )
			{
				playerStrikes = new PlayerStrikeInfo();
				playerStrikes.OriginalNpcType = npc.type;//used to help determine despawns.
				 //npcStrikes.Add(npcIndex, playerStrikes);
				npcStrikes.TryAdd(npcIndex, playerStrikes);
			}

			var realDamage = CalculateNpcDamage(npc, damage, isCritical);
			var damageDefended = (int)(CalculateNpcDefense(npc) * ( Main.expertMode ? 0.75f : 0.50f ));
			
			//Debug.Print($"Banking - realDamage: {realDamage}, Critical: {isCritical}");

			//Debug.Print($"OnNpcStrike item: {itemName}");
			playerStrikes.AddStrike(player.name, realDamage, damageDefended, itemName);
		}

		public void OnNpcKilled(NPC npc, int spawnHp)
		{
			var npcIndex = npc.whoAmI;

			if( npcStrikes.TryGetValue(npcIndex, out var strikes) )
			{
				//npcStrikes.Remove(npcIndex);
				npcStrikes.TryRemove(npcIndex, out var psi);
				if(StruckNpcKilled!=null)
				{
					var args = new StruckNpcKilledEventArgs(npc,spawnHp,strikes);
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
				var npc = Main.npc[npcIndex];

				if( npc?.active != true )
				{
					Debug.Print($"Npc #{npcIndex} despawned. Removing from strike tracking.");
					//npcStrikes.Remove(npcIndex);
					npcStrikes.TryRemove(npcIndex, out var psi);
				}
			}
		}
	}
}
