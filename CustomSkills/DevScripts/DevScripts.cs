using Corruption;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Built-in level definition scripts, for development
	/// </summary>
	public static class DevScripts
	{
		public static class TestSkill
		{
			public static void OnCast(TSPlayer player)
			{
				player.SendMessage("You begin to cast TestSkill!",Color.Purple);
			}

			public static void OnCharging(TSPlayer player,float completion)
			{
				if(completion>50f && completion<51f)
					player.SendMessage("Half way done...",Color.Purple);
			}

			public static void OnFire(TSPlayer player)
			{
				player.SendMessage("TestSkill has fired!",Color.Purple);
			}
		}

		public static class WindBreaker
		{
			public static void OnCast(TSPlayer player)
			{
				player.SendMessage("You feel an evil force stir inside you...", Color.Purple);
			}

			public static void OnCharging(TSPlayer player, float completion)
			{
				if((int)completion % 20 == 0 )
					player.SendMessage("Urgh...", Color.Purple);
			}

			public static void OnFire(TSPlayer player)
			{
				//player.SendMessage("BRWWAAAAAAPPPPPPPPPP!", Color.Green);
				PlayerFunctions.Broadcast("BRWWAAAAAAPPPPPPPPPP!", Color.Green);

				var radius = 16 * 16;
				var pos = player.TPlayer.position;
				var npcs = NpcFunctions.FindNpcsInRadius(pos.X, pos.Y, radius);
				var seconds = 25;

				player.SetBuff(120, 10 * 60);

				//var confuseCounter = 3;
			
				for(var i=0; i < Main.maxNPCs; i++)
				{
					var npc = Main.npc[i];

					if(!npc.active)
						continue;

					//confuseCounter--;

					//if(confuseCounter==0)
					//{
					//	confuseCounter = 3;
					//	npc.AddBuff(31, seconds * 60);
					//}
					//else
					npc.AddBuff(20, seconds * 60);

					var npcPos = npc.position;

					var newVelocity = (npc.position - pos);

					newVelocity.Normalize();
					newVelocity *= 50;

					npc.velocity = newVelocity;

					//npc.oldPosition = npc.position;
					//npc.position = npc.position + newVelocity;

					try
					{
						TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);//, npcPos.X, npcPos.Y, newVelocity.X, newVelocity.Y);
					}
					catch(Exception ex)
					{
						Debugger.Break();
					}

				}
			}
		}
	}
}
