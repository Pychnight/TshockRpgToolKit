//#define USE_FAST_ID_LOOKUP

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace Corruption
{
	public static class NpcFunctions
	{
		/// <summary>
		///     Spawns the mob with the specified name, coordinates, and amount.
		/// </summary>
		/// <param name="nameOrType">The name or type, which must be a valid NPC name or type and not <c>null</c>.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		/// <param name="radius">The radius, which must be positive.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <returns>The spawned NPCs.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="nameOrType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="nameOrType" /> is not a valid NPC name.</exception>
		public static NPC[] SpawnNpc(string nameOrType, int x, int y, int radius, int amount)
		{
			if (nameOrType == null)
				throw new ArgumentNullException(nameof(nameOrType));

			var npcId = GetNpcIdFromNameOrType(nameOrType);
			if (npcId == null)
				throw new FormatException($"Invalid NPC name '{nameOrType}'.");

			return SpawnNpc((int)npcId, x, y, radius, amount);
		}

		[Obsolete("Use SpawnNpc(nameOrType,x,y,radius,amount) instead.")]
		public static NPC[] SpawnMob(string nameOrType, int x, int y, int radius = 10, int amount = 1) => SpawnNpc(nameOrType,x,y,radius,amount);

		/// <summary>
		///     Spawns the NPC with the specified name, coordinates, and amount.
		/// </summary>
		/// <param name="type">The type, which must be a valid NPC type.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		/// <param name="radius">The radius, which must be positive.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <returns>The spawned NPCs.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		public static NPC[] SpawnNpc(int type, int x, int y, int radius, int amount)
		{
			if (radius <= 0)
				throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");

			if (amount <= 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

			var npcs = new List<NPC>(amount);
			for (var i = 0; i < amount; ++i)
			{
				TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
				var npcIndex = NPC.NewNPC(16 * spawnX, 16 * spawnY, type);
				if (npcIndex != Main.maxNPCs)
				{
					npcs.Add(Main.npc[npcIndex]);
				}
			}
			return npcs.ToArray();
		}

		[Obsolete("Use SpawnNpc(type,x,y,radius,amount) instead.")]
		public static NPC[] SpawnMob(int type, int x, int y, int radius = 10, int amount = 1) => SpawnNpc(type, x, y, radius, amount);
		

#if USE_FAST_ID_LOOKUP

		static Dictionary<string, int> npcNameToId;

		static NpcFunctions()
		{
			//generate dict for fast lookups of id's 
			npcNameToId = new Dictionary<string, int>();
			for( var i = -65; i < Main.maxNPCTypes; ++i )
			{
				var npcName = EnglishLanguage.GetNpcNameById(i);
				if(npcName!=null)
				{
					var lower = npcName.ToLower();
					npcNameToId.Add(lower, i);
				}
			}
		}
		
		static int? getNpcTypeFromNameImpl(string name)
		{
			var lower = name.ToLower();

			if(npcNameToId.TryGetValue(lower,out var result))
				return result;
			else
				return null;
		}

#else

		//id should really be called type -- ugh @ terraria
		static int? getNpcTypeFromNameImpl(string name)
		{
			for( var i = -65; i < Main.maxNPCTypes; ++i )
			{
				var npcName = EnglishLanguage.GetNpcNameById(i);
				if( npcName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false )
				{
					return i;
				}
			}

			return null;
		}

#endif

		public static int? GetNpcIdFromName(string name)
		{
			if( string.IsNullOrWhiteSpace(name) )
				return null;

			return getNpcTypeFromNameImpl(name);
		}
				
		public static int? GetNpcIdFromNameOrType(string nameOrType)
		{
			if( string.IsNullOrWhiteSpace(nameOrType) )
				return null;
						
			if( int.TryParse(nameOrType, out var id) && -65 <= id && id < Main.maxNPCTypes )
				return id;

			return getNpcTypeFromNameImpl(nameOrType);
		}
		
		/// <summary>
		/// Counts the number of NPCs in the area matching the name. (Uses GivenOrTypeName)
		/// </summary>
		public static int CountNpcs(int x, int y, int x2, int y2, string name)
		{
			var count = 0;
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);

			//normally, we'd use npc.Length to avoid bounds checks... but we're in Terraria, where nothing is as it should be.
			//maxNPCs = 200, but the npc array is sized at 201. ?!?!
			for( var i = 0; i < Main.maxNPCs; i++ )
			{ 
				var npc = Main.npc[i];
				if( npc.active && npc.position.X > 16 * minX && npc.position.X< 16 * maxX &&
				   npc.position.Y> 16 * minY && npc.position.Y< 16 * maxY && npc.GivenOrTypeName == name)
				{
					count++;
				}
			}
			return count;
		}

		///// <summary>
		///// Finds all NPCs in the area.
		///// </summary>
		//public static IList<NPC> FindNpcs(int x, int y, int x2, int y2)
		//{
		//	var result = new List<NPC>();
		//	var minX = Math.Min(x, x2);
		//	var maxX = Math.Max(x, x2);
		//	var minY = Math.Min(y, y2);
		//	var maxY = Math.Max(y, y2);

		//	//normally, we'd use npc.Length to avoid bounds checks... but we're in Terraria, where nothing is as it should be.
		//	//maxNPCs = 200, but the npc array is sized at 201. ?!?!
		//	for(var i = 0; i < Main.maxNPCs; i++)
		//	{
		//		var npc = Main.npc[i];
		//		if(npc.active && npc.position.X > 16 * minX && npc.position.X < 16 * maxX &&
		//		   npc.position.Y > 16 * minY && npc.position.Y < 16 * maxY )// && npc.GivenOrTypeName == name)
		//		{
		//			result.Add(npc);
		//		}
		//	}
		//	return result;
		//}

		/// <summary>
		/// Finds all npcs within a given radius of x, y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static List<NPC> FindNpcsInRadius(float x, float y, float radius)
		{
			var results = new List<NPC>();
			var center = new Vector2(x, y);

			foreach(var npc in Main.npc)
			{
				if(npc?.active == true)
				{
					var pos = npc.position;
					var delta = pos - center;

					if(delta.LengthSquared() <= (radius * radius))
						results.Add(npc);
				}
			}

			return results;//.AsReadOnly();
		}

		/// <summary>
		/// Finds an NPC by name. (Uses GivenOrTypeName) 
		/// </summary>
		/// <param name="name">GivenOrTypeName to find.</param>
		/// <returns>NPC if found, null if not.</returns>
		public static NPC FindNpcByName(string name)
		{ 
			for( var i = 0; i < Main.maxNPCs; i++ )
			{
				var npc = Main.npc[i];
				if( npc.active && npc.GivenOrTypeName == name )
					return npc;
			}

			return null;
		}

		/// <summary>
		/// Finds an NPC by type. 
		/// </summary>
		/// <param name="type">int type of NPC.</param>
		/// <returns>NPC if found, null if not.</returns>
		public static NPC FindNpcByType(int type)
		{
			for( var i = 0; i < Main.maxNPCs; i++ )
			{
				var npc = Main.npc[i];
				if( npc.active && npc.netID == type )
					return npc;
			}

			return null;
		}

		/// <summary>
		/// Sets a buff on an NPC.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="buffType"></param>
		/// <param name="buffTime"></param>
		public static void SetNpcBuff(NPC npc, int buffType, int buffTime) => SetNpcBuff(npc, buffType, buffTime, false);

		/// <summary>
		/// Sets a buff on an NPC.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="buffType"></param>
		/// <param name="buffTime"></param>
		/// <param name="quiet"></param>
		public static void SetNpcBuff(NPC npc, int buffType, int buffTime, bool quiet)
		{
			npc.AddBuff(buffType, buffTime, quiet);
			TSPlayer.All.SendData(PacketTypes.NpcAddBuff, "", npc.whoAmI, (float)buffType, (float)buffTime, 0f, 0);
		}

		/// <summary>
		/// Sends an npc update packet to all players for the given npc.
		/// </summary>
		/// <param name="npc"></param>
		public static void SendNpcUpdate(NPC npc) => SendNpcUpdate(TSPlayer.All, npc);
		
		/// <summary>
		/// Sends an npc update packet to the specified player, for the given npc.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="npc"></param>
		public static void SendNpcUpdate(TSPlayer player, NPC npc)
		{
			if(player == null)
				return;

			if(npc == null || npc.active == false)
				return;

			player.SendData(PacketTypes.NpcUpdate, "", npc.whoAmI);
		}
	}
}
