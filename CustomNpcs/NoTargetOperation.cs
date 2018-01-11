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
		private Random rnd = new Random();

		internal string PlayerName { get; set; }
						
		/// <summary>
		/// Ensures that NPC's will not target the TSPlayer with the set PlayerName. 
		/// </summary>
		internal void Ensure()
		{
			if( PlayerName == null )
				return;

			var noTargetPlayer = PlayerFunctions.FindPlayerByName(PlayerName);
			
			if( noTargetPlayer == null )
				return;
						
			foreach(var npc in Main.npc)
			{
				if(npc?.active==true && npc.target==noTargetPlayer.Index )
				{
					setNewTarget(npc,noTargetPlayer);
				}
			}
		}

		internal void Ensure(NPC npc)
		{
			if( PlayerName == null )
				return;

			var noTargetPlayer = PlayerFunctions.FindPlayerByName(PlayerName);

			if( noTargetPlayer == null )
				return;

			if( npc?.active == true && npc.target == noTargetPlayer.Index )
			{
				setNewTarget(npc, noTargetPlayer);
			}
		}

		private void setNewTarget(NPC npc, TSPlayer noTargetPlayer)
		{
			//Debug.Print("SetNewTarget!");
			const float radius = 16 * 50;//50 tiles
			const int noTargetDefaultIndex = -1;

			var nearbyPlayers = PlayerFunctions.FindPlayersInRadius(npc.position.X, npc.position.Y, radius);

			nearbyPlayers.Remove(noTargetPlayer);
			
			if(nearbyPlayers.Count>0)
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
