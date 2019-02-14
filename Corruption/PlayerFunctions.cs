using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;

namespace Corruption
{
	public static class PlayerFunctions
	{
		/// <summary>
		///		Broadcasts the specified message.
		/// </summary>
		/// <param name="message">The message string.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void Broadcast(string message)
		{
			Broadcast(message, Color.White);
		}

		/// <summary>
		///     Broadcasts the specified message.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="color">The color.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void Broadcast( string message, Color color)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			TShock.Utils.Broadcast(message, color);
		}

		/// <summary>
		///     Broadcasts the specified message.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void Broadcast( string message, byte r, byte g, byte b)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			TShock.Utils.Broadcast(message, r, g, b);
		}

		/// <summary>
		///     Bans Players
		/// </summary>
		/// <param name="player">The message, which must not be <c>null</c>.</param>
		/// <param name="message">The color.</param>
		/// <param name="message2">The color.</param>
		/// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
		public static void Ban(TSPlayer player, string message, string message2)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(player));
			
			TShock.Utils.Ban(player, message, true, message2);
		}

		/// <summary>
		///	Sends a combat text with the specified color and position to a given player.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <param name="position"></param>
		public static void CreateCombatText(TSPlayer player, string text, Color color, Vector2 position)
		{
			player.SendData((PacketTypes)119, text, (int)color.PackedValue, position.X, position.Y);
		}

		/// <summary>
		/// Sends a combat text with the specified color and position to all players.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="color">The color.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="text" /> is <c>null</c>.</exception>
		public static void CreateCombatText(string text, Color color, Vector2 position)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			
			TSPlayer.All.SendData((PacketTypes)119, text, (int)color.PackedValue, position.X, position.Y);
		}

		//we stripped attacker from calling functions for now, but in case it gets readded again later we keep it.
		static PlayerDeathReason createPlayerDeathReason(object attacker, string reason)
		{
			if( string.IsNullOrWhiteSpace(reason) )
				return PlayerDeathReason.LegacyDefault();
			else
			{
				var result = new PlayerDeathReason();

				result.SourceCustomReason = reason;

				//Disabled following when we created CorruptionLib, since it has no references to CustomNpcs, and the attacker field currently goes unused.
				//will need to reinvestigate how to handle this best.

				//var npc = attacker as CustomNpc;
				//if( npc != null )
				//{
				//	result.SourceNPCIndex = npc.Index;
				//}
				//else
				//{
				//	var projectile = attacker as CustomProjectile;
				//	if( projectile != null )
				//	{
				//		result.SourceProjectileIndex = projectile.Index;
				//	}
				//}

				return result;
			}
		}

		public static void HurtPlayer(TSPlayer player, int damage, bool critical)
		{
			HurtPlayer(player, damage, critical, null);
		}
		
		public static void HurtPlayer(TSPlayer player, int damage, bool critical, string deathReason)
		{
			var reason = createPlayerDeathReason(null, deathReason);
			var dir = new Random().Next(-1, 1);

			//this isnt working, for whatever reason.
			//player.TPlayer.Hurt(reason, damage, dir, false, false, critical);

			//so we use what player.DamagePlayer(damage) does...
			
			//NetMessage.SendPlayerHurt(player.Index, reason, damage, 0, critical, false, 0, -1, -1);
			NetMessage.SendPlayerHurt(player.Index, reason, damage, dir, critical, false, 0, -1, -1);
		}

		public static void KillPlayer(TSPlayer player)
		{
			KillPlayer(player, null);
		}

		public static void KillPlayer(TSPlayer player, string deathReason) //, bool pvp = false)
		{
			var reason = createPlayerDeathReason(null, deathReason);
			
			//player.TPlayer.KillMe(reason, 99999, new Random().Next(-1, 1), false);
			//player.KillPlayer();
			NetMessage.SendPlayerDeath(player.Index, reason, 99999, new Random().Next(-1, 1), false, -1, -1);
		}

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

		//not sure if we should expose to scripts...
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

		//not sure if we should expose to scripts...
		public static TSPlayer FindPlayerByName(string name)
		{
			var player = TShock.Players.Where(n => n?.Name == name).SingleOrDefault();
			return player;
		}
		
		/// <summary>
		/// Returns whether the topmost region at (x,y) contains a minimum amount of players.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="minAmount"></param>
		/// <returns></returns>
		public static bool PlayerAmountInRegion(string regionName, int minAmount)
		{
			if( minAmount < 0 )
				return true;

			var region = TShock.Regions.GetRegionByName(regionName);

			if(region!=null)
			{
				var activePlayers = TShock.Players.Where(p => p?.Active == true);
				
				foreach(var player in activePlayers)
				{
					if(region.InArea(player.TileX, player.TileY))
					{
						minAmount--;

						if( minAmount < 1 )
							break;
					}
				}
			}

			return minAmount<1;
		}
		
		/// <summary>
		/// Returns whether the minimum amount of players are currently connected and active.
		/// </summary>
		/// <param name="minAmount"></param>
		/// <returns></returns>
		public static bool PlayerAmountConnected(int minAmount)
		{
			foreach(var p in TShock.Players)
			{
				if( p.Active )
				{
					minAmount--;

					if( minAmount < 1 )
						break;
				}
			}

			return minAmount<1;
		}

		/// <summary>
		/// Returns whether the player has all of the supplied permissions.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="permissions">String params, or array of permission strings.</param>
		/// <returns></returns>
		public static bool PlayerHasPermission(TSPlayer player, params string[] permissions)
		{
			if(permissions==null || permissions.Length < 1 )
				return false;

			if( player == null )
				return false;

			foreach( var perm in permissions )
			{
				if( perm != null && player?.HasPermission(perm) != true )
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns whether the player is part of the named groups.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="group">Group name string.</param>
		/// <returns>True on success, false otherwise.</returns>
		public static bool PlayerHasGroup(TSPlayer player, string group)
		{
			var playerGroup = player.Group;

			while( playerGroup != null )
			{
				if( playerGroup.Name == group )
					return true;

				playerGroup = playerGroup.Parent;
			}

			return false;
		}

		/// <summary>
		/// Returns whether the player is part of all of the named groups.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="groups">Group name strings, as params or an array.</param>
		/// <returns>True on success, false otherwise.</returns>
		public static bool PlayerHasGroup(TSPlayer player, params string[] groups)
		{
			if( groups == null || groups.Length < 1 )
				return false;
			
			foreach(var g in groups)
			{
				if( g!=null && !PlayerHasGroup(player, g) )
					return false;
			}

			return true;
		}
	}
}
