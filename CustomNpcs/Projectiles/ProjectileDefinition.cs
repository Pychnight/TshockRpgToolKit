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
		internal BaseOverrideDefinition BaseOverride { get; set; } = new BaseOverrideDefinition();

		/// <summary>
		///     Gets a function that is invoked when the projectile AI is spawned.
		/// </summary>
		public SafeLuaFunction OnSpawn { get; private set; }

		/// <summary>
		/// Gets a function that is invoked when the projectile becomes inactive.
		/// </summary>
		public SafeLuaFunction OnKilled { get; private set; }

		/// <summary>
		///     Gets a function that is invoked for the projectile on each game update.
		/// </summary>
		public SafeLuaFunction OnGameUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the projectile AI is updated.
		/// </summary>
		public SafeLuaFunction OnAiUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the projectile collides with a player.
		/// </summary>
		public SafeLuaFunction OnCollision { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the projectile collides with a player.
		/// </summary>
		public SafeLuaFunction OnTileCollision { get; private set; }

		public void Dispose()
		{
			OnSpawn = null;
			OnKilled = null;
			OnGameUpdate = null;
			OnAiUpdate = null;
			OnCollision = null;
			OnTileCollision = null;
			
			lua?.Dispose();
			lua = null;
		}

		public void ApplyTo(Projectile projectile)
		{
			if(projectile == null)
			{
				throw new ArgumentNullException(nameof(projectile));
			}

			//projectile.type = 0;
			projectile.aiStyle = BaseOverride.AiStyle ?? projectile.aiStyle;
			projectile.ai = BaseOverride.Ai ?? projectile.ai;
			projectile.damage = BaseOverride.Damage ?? projectile.damage;
			projectile.knockBack = BaseOverride.KnockBack ?? projectile.knockBack;
			projectile.friendly = BaseOverride.Friendly ?? projectile.friendly;
			projectile.hostile = BaseOverride.Hostile ?? projectile.hostile;
			projectile.maxPenetrate = BaseOverride.MaxPenetrate ?? projectile.maxPenetrate;
			projectile.timeLeft = BaseOverride.TimeLeft ?? projectile.timeLeft;
			projectile.width = BaseOverride.Width ?? projectile.width;
			projectile.height = BaseOverride.Height ?? projectile.height;
			projectile.magic = BaseOverride.Magic ?? projectile.magic;
			projectile.light = BaseOverride.Light ?? projectile.light;
			projectile.thrown = BaseOverride.Thrown ?? projectile.thrown;
			projectile.melee = BaseOverride.Melee ?? projectile.melee;
			projectile.coldDamage = BaseOverride.ColdDamage ?? projectile.coldDamage;
			projectile.tileCollide = BaseOverride.TileCollide ?? projectile.tileCollide;
			projectile.ignoreWater = BaseOverride.IgnoreWater ?? projectile.ignoreWater;
			//projectile.wet = baseOverride.Wet ?? projectile.wet;
			projectile.bobber = BaseOverride.Bobber ?? projectile.bobber;
			projectile.counterweight = BaseOverride.Counterweight ?? projectile.counterweight;
			//projectile.hide = false;
			//projectile.honeyWet = false;
			//projectile.miscText = "test";
			//projectile.noEnchantments = false;
			//projectile.rotation = 0f;
			//projectile.scale = 1.0f;
			//projectile.sentry = false;
			//projectile.spriteDirection = 1;
			//projectile.velocity = new Vector2(1, 1);
			//projectile.wet = false;
			//projectile.wetCount = 0;
			//projectile.melee = false;
			//projectile.oldVelocity;
			//projectile.velocity;
			//projectile.oldPosition;
			//projectile.position;
			//projectile.numHits = 0;
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
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(ProjectileFunctions));
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(TileFunctions));
			
			lua["TileSize"] = TileFunctions.TileSize;
			lua["HalfTileSize"] = TileFunctions.HalfTileSize;
			lua["Center"] = new CenterOffsetHelper();

			lua.DoFile(Path.Combine("npcs", LuaPath));
			
			OnAiUpdate = lua.GetSafeFunction("OnAiUpdate");
			OnGameUpdate = lua.GetSafeFunction("OnGameUpdate");
			OnCollision = lua.GetSafeFunction("OnCollision");
			OnTileCollision = lua.GetSafeFunction("OnTileCollision");
			OnKilled = lua.GetSafeFunction("OnKilled");
			OnSpawn = lua.GetSafeFunction("OnSpawn");
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
			//if( string.IsNullOrWhiteSpace(Name) )
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
			if( LuaPath != null && !File.Exists(Path.Combine("npcs", LuaPath)) )
			{
				throw new FormatException($"{nameof(LuaPath)} points to an invalid Lua file.");
			}
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
			public int? MaxPenetrate { get; set; }

			[JsonProperty]
			public int? TimeLeft { get; set; }
			
			[JsonProperty]
			public int? Width { get; set;}
			
			[JsonProperty]
			public int? Height { get; set;}
			
			[JsonProperty]
			public bool? Magic { get; set; }

			[JsonProperty]
			public float? Light { get; set; }
			
			[JsonProperty]
			public bool? Thrown { get; set; }

			[JsonProperty]
			public bool? Melee { get; set; }
			
			[JsonProperty]
			public bool? ColdDamage { get; set; }
			
			[JsonProperty]
			public bool? TileCollide { get; set; }

			[JsonProperty]
			public bool? IgnoreWater { get; set; }
			
		/* 	[JsonProperty]
			public bool? Wet { get; set; } */

			[JsonProperty]
			public bool? Bobber { get; set; }
			
			[JsonProperty]
			public bool? Counterweight { get; set; }
		}
	}
}

	
