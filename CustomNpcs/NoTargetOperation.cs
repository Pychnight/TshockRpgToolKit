using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace CustomNpcs
{
	/// <summary>
	/// Backing implementation for the /notarget command.
	/// </summary>
	internal class NoTargetOperation
	{
		private Random rnd;
		private Dictionary<int, TSPlayer> cachedNoTargetPlayers;//we cache this to help minimize allocations, but it is repopulated per call to Ensure().
		
		internal HashSet<string> PlayerNames { get; private set; }
		
		internal NoTargetOperation()
		{
			rnd = new Random();
			PlayerNames = new HashSet<string>();
			cachedNoTargetPlayers = new Dictionary<int, TSPlayer>();
		}

		internal bool Add(string playerName)
		{
			return PlayerNames.Add(playerName);
		}

		internal bool Remove(string playerName)
		{
			return PlayerNames.Remove(playerName);
		}

		internal void Clear()
		{
			PlayerNames.Clear();
		}

		/// <summary>
		/// Returns an IEnumerable of TSPlayer's that match the notarget list.
		/// </summary>
		/// <returns>IEnumerable of TSPlayers</returns>
		internal IEnumerable<TSPlayer> EnumerateNoTargetPlayers()
		{
			var players = TShock.Players.Where(p => p?.Active == true && PlayerNames.Contains(p.Name));
			return players;
		}
		
		/// <summary>
		/// Returns a Dictionary of TSPlayers on the notarget list, keyed by their index.
		/// </summary>
		/// <returns>Dictionary.</returns>
		internal Dictionary<int,TSPlayer> FindNoTargetPlayers()
		{
			if( PlayerNames.Count < 1 )
				return cachedNoTargetPlayers;

			cachedNoTargetPlayers.Clear();

			var players = EnumerateNoTargetPlayers();
			players.ForEach(p => cachedNoTargetPlayers[p.Index] = p);
													
			return cachedNoTargetPlayers;
		}

		/// <summary>
		/// Trys to ensure that NPC's targeting players on the no target list will lose their targeting.
		/// </summary>
		internal void Ensure()
		{
			var noTargetPlayers = FindNoTargetPlayers();

			if( noTargetPlayers.Count<1 )
				return;
			
			foreach( var npc in Main.npc )
			{
				if( npc?.active == true && noTargetPlayers.ContainsKey(npc.target) )
				{
					setNewTarget(npc, noTargetPlayers);
				}
			}
		}

		private void setNewTarget(NPC npc, Dictionary<int,TSPlayer> noTargetPlayers)
		{
			const float radius = 16 * 50;//50 tiles
			const int noTargetDefaultIndex = -1;

			var nearbyPlayers = PlayerFunctions.FindPlayersInRadius(npc.position.X, npc.position.Y, radius);

			nearbyPlayers.RemoveAll(np => noTargetPlayers.ContainsKey(np.Index));

			if( nearbyPlayers.Count > 0 )
			{
				var index = rnd.Next(0, nearbyPlayers.Count);
				var newTargetPlayer = nearbyPlayers[index];

				Debug.Print($"setNewTarget: {newTargetPlayer.Index}");
				npc.target = newTargetPlayer.Index;
			}
			else
			{
				Debug.Print($"setNewTarget: {noTargetDefaultIndex}");
				npc.target = noTargetDefaultIndex;
			}

			TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.whoAmI);
			npc.netUpdate = true;
		}
	}
}
