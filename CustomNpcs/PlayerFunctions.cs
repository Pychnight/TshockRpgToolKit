using CustomNpcs.Npcs;
using CustomNpcs.Projectiles;
using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;

namespace CustomNpcs
{
	public static class PlayerFunctions
	{
		//we stripped attacker from calling functions for now, but in case it gets readded again later we keep it.
		static PlayerDeathReason createPlayerDeathReason(object attacker, string reason)
		{
			if( string.IsNullOrWhiteSpace(reason) )
				return PlayerDeathReason.LegacyDefault();
			else
			{
				var result = new PlayerDeathReason();

				result.SourceCustomReason = reason;

				var npc = attacker as CustomNpc;
				if( npc != null )
				{
					result.SourceNPCIndex = npc.Index;
				}
				else
				{
					var projectile = attacker as CustomProjectile;
					if( projectile != null )
					{
						result.SourceProjectileIndex = projectile.Index;
					}
				}

				return result;
			}
		}
		
		[LuaGlobal]
		public static void HurtPlayer(TSPlayer player, int damage, bool critical, string deathReason)
		{
			var reason = createPlayerDeathReason(null, deathReason);

			//this isnt working, for whatever reason.
			//player.TPlayer.Hurt(reason, damage, hitDirection, false, quiet, crit, cooldownCounter);

			//so we use what player.DamagePlayer(damage) does...
			NetMessage.SendPlayerHurt(player.Index, reason, damage, new Random().Next(-1, 1), critical, false, 0, -1, -1);
		}

		[LuaGlobal]
		public static void KillPlayer(TSPlayer player, string deathReason) //, bool pvp = false)
		{
			var reason = createPlayerDeathReason(null, deathReason);
			
			//player.TPlayer.KillMe(reason, damage, new Random().Next(-1, 1), false);

			//player.KillPlayer();
			NetMessage.SendPlayerDeath(player.Index, reason, 99999, new Random().Next(-1, 1), false, -1, -1);
		}

		[LuaGlobal]
		public static void RadialHurtPlayer(int x, int y, int radius, int damage, float falloff, string deathReason)
		{
			if( radius < 1 )
				return;

			var center = new Vector2(x, y);
			//falloff = Math.Min(falloff, 1.0f);//clip to 1

			foreach( var player in FindPlayersInRadius(x, y, radius) )
			{
				var pos = new Vector2(player.X, player.Y);
				var delta = pos - center;
				var dist = delta.LengthSquared();
				var damageStep = dist / ( radius * radius );

				var adjustedDamage = damage * ( 1.0f - ( damageStep * falloff ) );

				if( adjustedDamage > 1 )
				{
					//player.DamagePlayer((int)adjustedDamage);
					HurtPlayer(player, (int)adjustedDamage, false, deathReason);
				}
					

				//...internally, DamagePlayer looks like this: 
				//NetMessage.SendPlayerHurt(this.Index, PlayerDeathReason.LegacyDefault(), damage, new Random().Next(-1, 1), false, false, 0, -1, -1);

				//might be able to do our knockback effect?	
			}
		}

		//not sure if we should expose following methods to lua...

		public static List<TSPlayer> FindPlayersInRadius(float x, float y, float radius)
		{
			var results = new List<TSPlayer>();
			var center = new Vector2(x, y);

			foreach( var player in TShock.Players )
			{
				if( player?.Active==true )
				{
					var pos = new Vector2(player.X, player.Y);
					var delta = pos - center;

					if( delta.LengthSquared() <= ( radius * radius ) )
						results.Add(player);
				}
			}

			return results;//.AsReadOnly();
		}

		public static TSPlayer FindPlayerByName(string name)
		{
			var player = TShock.Players.Where(n => n?.Name == name).SingleOrDefault();
			return player;
		}
	}
}
