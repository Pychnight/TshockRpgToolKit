using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomNpcs.Projectiles
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ProjectileDefinition : IDisposable
	{
		Lua lua;

		[JsonProperty(Order = 0)]
		public string Name { get; set; }

		[JsonProperty(Order = 1)]
		public string LuaPath { get; set; }

		[JsonProperty(Order = 2)]
		public int BaseType { get; set; }

		[JsonProperty("BaseOverride",Order = 3)]
		internal BaseOverrideDefinition baseOverride { get; set; } = new BaseOverrideDefinition();

		/// <summary>
		///     Gets a function that is invoked when the projectile AI is spawned.
		/// </summary>
		public LuaFunction OnSpawn { get; private set; }

		/// <summary>
		/// Gets a function that is invoked when the projectile becomes inactive.
		/// </summary>
		public LuaFunction OnKilled { get; private set; }

		/// <summary>
		///     Gets a function that is invoked for the projectile on each game update.
		/// </summary>
		public LuaFunction OnGameUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the projectile AI is updated.
		/// </summary>
		public LuaFunction OnAiUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the projectile collides with a player.
		/// </summary>
		public LuaFunction OnCollision { get; private set; }

		public void Dispose()
		{
			OnSpawn = null;
			OnKilled = null;
			OnGameUpdate = null;
			OnAiUpdate = null;
			OnCollision = null;
			
			lua?.Dispose();
			lua = null;
		}

		public void ApplyTo(Projectile projectile)
		{
			if(projectile == null)
			{
				throw new ArgumentNullException(nameof(projectile));
			}

			//// Set NPC to use four life bytes.
			//Main.npcLifeBytes[BaseType] = 4;

			//if (npc.netID != BaseType)
			//{
			//	npc.SetDefaults(BaseType);
			//}
			//npc.aiStyle = _baseOverride.AiStyle ?? npc.aiStyle;
			//if (_baseOverride.BuffImmunities != null)
			//{
			//	for (var i = 0; i < Main.maxBuffTypes; ++i)
			//	{
			//		npc.buffImmune[i] = false;
			//	}
			//	foreach (var i in _baseOverride.BuffImmunities)
			//	{
			//		npc.buffImmune[i] = true;
			//	}
			//}
			//npc.defense = npc.defDefense = _baseOverride.Defense ?? npc.defense;
			//npc.noGravity = _baseOverride.HasNoGravity ?? npc.noGravity;
			//npc.noTileCollide = _baseOverride.HasNoCollision ?? npc.noTileCollide;
			//npc.boss = _baseOverride.IsBoss ?? npc.boss;
			//npc.immortal = _baseOverride.IsImmortal ?? npc.immortal;
			//npc.lavaImmune = _baseOverride.IsImmuneToLava ?? npc.lavaImmune;
			//npc.trapImmune = _baseOverride.IsTrapImmune ?? npc.trapImmune;
			//// Don't set npc.lifeMax so that the correct life is always sent to clients.
			//npc.knockBackResist = _baseOverride.KnockbackMultiplier ?? npc.knockBackResist;
			//npc.life = _baseOverride.MaxHp ?? npc.life;
			//npc._givenName = _baseOverride.Name ?? npc._givenName;
			//npc.npcSlots = _baseOverride.NpcSlots ?? npc.npcSlots;
			//npc.value = _baseOverride.Value ?? npc.value;

			//start our cargo cult version...

			projectile.aiStyle = baseOverride.AiStyle ?? projectile.aiStyle;
			projectile.ai = baseOverride.Ai ?? projectile.ai;
			projectile.damage = baseOverride.Damage ?? projectile.damage;
			projectile.knockBack = baseOverride.KnockBack ?? projectile.knockBack;
			projectile.friendly = baseOverride.Friendly ?? projectile.friendly;
			projectile.hostile = baseOverride.Hostile ?? projectile.hostile;
			projectile.coldDamage = baseOverride.ColdDamage ?? projectile.coldDamage;
			projectile.tileCollide = baseOverride.TileCollide ?? projectile.tileCollide;
			projectile.timeLeft = baseOverride.TimeLeft ?? projectile.timeLeft;
			projectile.maxPenetrate = baseOverride.MaxPenetrate ?? projectile.maxPenetrate;
			projectile.ignoreWater = baseOverride.IgnoreWater ?? projectile.ignoreWater;
			//
			//projectile.light = 0.0f;//
			//projectile.magic = false;


			//projectile.rotation = 0f;
			//projectile.scale = 1.0f;
			//projectile.sentry = false;
			//projectile.spriteDirection = 1;
			//projectile.thrown = true;

			//projectile.type = 0;
			//projectile.velocity = new Vector2(1, 1);
			//projectile.wet = false;
			//projectile.wetCount = 0;
			//projectile.width = 10;
			//projectile.height = 10;
			//projectile.hide = false;
			//projectile.honeyWet = false;
			//projectile.miscText = "test";
			//projectile.melee = false;
			//projectile.oldVelocity;
			//projectile.velocity;
			//projectile.oldPosition;
			//projectile.position;
			//projectile.numHits = 0;
			//projectile.noEnchantments = false;
			//projectile.counterweight = false;
			//projectile.bobber = false;
			//projectile.alpha = 1;
			//projectile.direction = 0;
		}

		/// <summary>
		///     Loads the Lua definition, if possible.
		/// </summary>
		public void LoadLuaDefinition()
		{
			if(LuaPath == null)
			{
				return;
			}

			var lua = this.lua = new Lua();
			lua.LoadCLRPackage();
			lua.DoString("import('System')");
			lua.DoString("import('OTAPI', 'Microsoft.Xna.Framework')");
			lua.DoString("import('OTAPI', 'Terraria')");
			lua.DoString("import('TShock', 'TShockAPI')");
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(NpcFunctions));
			lua.DoFile(Path.Combine("npcs", LuaPath));
			
			OnAiUpdate = lua["OnAiUpdate"] as LuaFunction;
			OnGameUpdate = lua["OnGameUpdate"] as LuaFunction;
			OnCollision = lua["OnCollision"] as LuaFunction;
			OnKilled = lua["OnKilled"] as LuaFunction;
			OnSpawn = lua["OnSpawn"] as LuaFunction;
			//OnStrike = _lua["OnStrike"] as LuaFunction;
		}

		internal void ThrowIfInvalid()
		{
			if (Name == null)
			{
				throw new FormatException($"{nameof(Name)} is null.");
			}
			//if (int.TryParse(Name, out _))
			//{
			//	throw new FormatException($"{nameof(Name)} cannot be a number.");
			//}
			//if (string.IsNullOrWhiteSpace(Name))
			//{
			//	throw new FormatException($"{nameof(Name)} is whitespace.");
			//}
			//if (BaseType < -65)
			//{
			//	throw new FormatException($"{nameof(BaseType)} is too small.");
			//}
			//if (BaseType >= Main.maxNPCTypes)
			//{
			//	throw new FormatException($"{nameof(BaseType)} is too large.");
			//}
			//if (LuaPath != null && !File.Exists(Path.Combine("npcs", LuaPath)))
			//{
			//	throw new FormatException($"{nameof(LuaPath)} points to an invalid Lua file.");
			//}
			//if (_loot == null)
			//{
			//	throw new FormatException("Loot is null.");
			//}
			//_loot.ThrowIfInvalid();
			//if (_spawning == null)
			//{
			//	throw new FormatException("Spawning is null.");
			//}
			//if (_baseOverride == null)
			//{
			//	throw new FormatException("BaseOverride is null.");
			//}
			//_baseOverride.ThrowIfInvalid();
		}

		[JsonObject(MemberSerialization.OptIn)]
		internal sealed class BaseOverrideDefinition
		{
			[JsonProperty]
			public int? AiStyle { get; set; }

			[JsonProperty]
			public float[] Ai { get; set; }

			[JsonProperty]
			public int? Damage { get; set; }

			[JsonProperty]
			public int? KnockBack { get; set; }

			[JsonProperty]
			public bool? Friendly { get; set; }
						
			[JsonProperty]
			public bool? Hostile { get; set; } 

			[JsonProperty]
			public bool? ColdDamage { get; set; }

			[JsonProperty]
			public bool? IgnoreWater { get; set; }

			[JsonProperty]
			public bool? TileCollide { get; set; }

			[JsonProperty]
			public int? MaxPenetrate { get; set; }

			[JsonProperty]
			public int? TimeLeft { get; set; }
		}
	}
}

	
