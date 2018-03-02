using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace NpcShops
{
	public class NpcPauser
	{
		Stopwatch clock;
		Dictionary<int, PausedNpcInfo> pausedNpcs { get; set; }

		public NpcPauser()
		{
			clock = Stopwatch.StartNew();
			pausedNpcs = new Dictionary<int, PausedNpcInfo>();
		}
		
		public void Pause(NPC npc, int durationMS = -1, bool replaceExisting = true)
		{
			var index = npc.whoAmI;
			PausedNpcInfo info;
			
			if(pausedNpcs.TryGetValue(index,out info))
			{
				if( replaceExisting )
				{
					info.StartTime = clock.ElapsedMilliseconds;
					info.Duration = durationMS;
				}
				//else, ignore the update
				return;
			}
			else
			{
				info = new PausedNpcInfo()
				{
					StartTime = clock.ElapsedMilliseconds,
					Duration = durationMS,
					PreviousAiStyle = npc.aiStyle
				};

				Debug.Print($"Pause NPC #{npc.whoAmI}");

				npc.velocity = Vector2.Zero;

				var x = npc.position.X;
				var y = npc.position.Y;
				var vx = 0;
				var vy = 0;
				var flags = 4 + 8 + 16 + 32;
				//var flags = 4;
				
				npc.ai[0] = 0;
				npc.ai[1] = 0;
				npc.ai[2] = 0;
				npc.ai[3] = 0;
								
				npc.aiStyle = 0;
				NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.FromLiteral(""), npc.whoAmI, x, y, vx, vy, flags);

				pausedNpcs.Add(index, info);
			}
		}

		public void Unpause(NPC npc)
		{
			if(pausedNpcs.TryGetValue(npc.whoAmI, out var info))
			{
				Debug.Print($"Unpause NPC #{npc.whoAmI}");
				pausedNpcs.Remove(npc.whoAmI);
				
				npc.aiStyle = info.PreviousAiStyle;
				NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.FromLiteral(""), npc.whoAmI);
			}
		}

		public void UnpauseAll()
		{
			var kvps = pausedNpcs.ToArray();

			foreach( var kvp in kvps )
			{
				var npc = Main.npc[kvp.Key];
				var info = kvp.Value;

				//remove any despawned npcs
				if( npc?.active == true )
				{
					Unpause(npc);
				}
			}
		}
		
		public void OnGameUpdate()
		{
			var kvps = pausedNpcs.ToArray();
			
			foreach(var kvp in kvps )
			{
				var npc = Main.npc[kvp.Key];
				var info = kvp.Value;

				//remove any despawned npcs
				if(npc == null || npc.active==false)
				{
					pausedNpcs.Remove(kvp.Key);
					continue;
				}

				//set to unlimited pause.
				if( info.Duration == -1 )
					continue;

				if(clock.ElapsedMilliseconds - info.StartTime >= info.Duration )
					Unpause(npc);
			}
		}

		private class PausedNpcInfo
		{
			//internal int NpcIndex;
			//internal int NpcType;
			internal int PreviousAiStyle;
			internal long StartTime;
			internal int Duration;
		}
	}
}
