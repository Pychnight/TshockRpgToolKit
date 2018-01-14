using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua;
using Terraria;
using CustomNpcs.Projectiles;
using System.Reflection;
using TShockAPI;
using System.Diagnostics;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents an NPC definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class NpcDefinition : IDisposable
    {
		//internal string originalName; //we need to capture the npc's original name before applying our custom name to it, so the exposed lua function
									  //NameContains() works...

		[JsonProperty("BaseOverride", Order = 3)]
        internal BaseOverrideDefinition _baseOverride = new BaseOverrideDefinition();

        [JsonProperty("Loot", Order = 4)]
        private LootDefinition _loot = new LootDefinition();

        internal Lua _lua;//internal so that our NameContains/OnCheckSpawn global hack works.

        [JsonProperty("Spawning", Order = 5)]
        private SpawningDefinition _spawning = new SpawningDefinition();

        /// <summary>
        ///     Gets the base type.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int BaseType { get; private set; }

        /// <summary>
        ///     Gets the loot entries.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public List<LootEntryDefinition> LootEntries => _loot.Entries;

        /// <summary>
        ///     Gets the Lua path.
        /// </summary>
        [CanBeNull]
        [JsonProperty(Order = 1)]
        public string LuaPath { get; private set; }

		[CanBeNull]
		[JsonProperty(Order = 6)]
		public string BooPath { get; private set; }

		/// <summary>
		///     Gets the internal name.
		/// </summary>
		[JsonProperty(Order = 0)]
        [NotNull]
        public string Name { get; private set; } = "example";

        /// <summary>
        ///     Gets a function that is invoked when the NPC AI is updated.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnAiUpdate { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is checked for replacing.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnCheckReplace { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is checked for spawning.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnCheckSpawn { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC collides with a player.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnCollision { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when NPC is killed.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnKilled { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is spawned.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnSpawn { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is struck.
        /// </summary>
        [CanBeNull]
        public SafeLuaFunction OnStrike { get; private set; }

		/// <summary>
		///     Gets a function that is invoked after the NPC has transformed.
		/// </summary>
		[CanBeNull]
		public SafeLuaFunction OnTransformed { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the NPC collides with a tile.
		/// </summary>
		public SafeLuaFunction OnTileCollision { get; private set; }

		public CheckReplaceHandler BooCheckReplace { get; set; }
		public CheckSpawnHandler BooCheckSpawn { get; set; }
		public SpawnHandler BooSpawn { get; set; }
		public CollisionHandler BooCollision { get; set; }
		public TileCollisionHandler BooTileCollision { get; set; }
		public KilledHandler BooKilled { get; set; }
		public TransformedHandler BooTransformed { get; set; }
		public StrikeHandler BooStrike { get; set; }
		public AiUpdateHandler BooAiUpdate { get; set; }		

		/// <summary>
		///     Gets a value indicating whether the NPC should aggressively update due to unsynced changes with clients.
		/// </summary>
		public bool ShouldAggressivelyUpdate =>
            _baseOverride.AiStyle != null || _baseOverride.BuffImmunities != null ||
            _baseOverride.IsImmuneToLava != null || _baseOverride.HasNoCollision != null ||
            _baseOverride.HasNoGravity != null;

        /// <summary>
        ///     Gets a value indicating whether loot should be overriden.
        /// </summary>
        public bool ShouldOverrideLoot => _loot.IsOverride;

        /// <summary>
        ///     Gets a value indicating whether the NPC should spawn.
        /// </summary>
        public bool ShouldReplace => _spawning.ShouldReplace;

        /// <summary>
        ///     Gets a value indicating whether the NPC should spawn.
        /// </summary>
        public bool ShouldSpawn => _spawning.ShouldSpawn;

		/// <summary>
		///     Gets an optional value that overrides the global spawnrate, if present.
		/// </summary>
		public int? SpawnRateOverride => _spawning.SpawnRateOverride;

		/// <summary>
		///     Gets a value indicating whether the NPC should have kills tallied.
		/// </summary>
		public bool ShouldTallyKills => _loot.TallyKills;

        /// <summary>
        ///     Gets a value indicating whether the NPC should update on hit.
        /// </summary>
        public bool ShouldUpdateOnHit =>
            _baseOverride.Defense != null || _baseOverride.IsImmortal != null ||
            _baseOverride.KnockbackMultiplier != null;

        /// <summary>
        ///     Disposes the definition.
        /// </summary>
        public void Dispose()
        {
            OnAiUpdate = null;
            OnCheckReplace = null;
            OnCheckSpawn = null;
            OnCollision = null;
			OnTileCollision = null;
			OnKilled = null;
            OnSpawn = null;
            OnStrike = null;
			OnTransformed = null;
			_lua?.Dispose();
            _lua = null;

			BooCheckReplace = null;
			BooCheckSpawn = null;
			BooSpawn = null;
			BooKilled = null;
			BooTransformed = null;
			BooCollision = null;
			BooTileCollision = null;
			BooStrike = null;
			BooAiUpdate = null;
		}

        /// <summary>
        ///     Applies the definition to the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public void ApplyTo([NotNull] NPC npc)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

			// Set NPC to use four life bytes.
			Main.npcLifeBytes[BaseType] = 4;

            if (npc.netID != BaseType)
            {
                npc.SetDefaults(BaseType);
            }
            npc.aiStyle = _baseOverride.AiStyle ?? npc.aiStyle;
            if (_baseOverride.BuffImmunities != null)
            {
                for (var i = 0; i < Main.maxBuffTypes; ++i)
                {
                    npc.buffImmune[i] = false;
                }
                foreach (var i in _baseOverride.BuffImmunities)
                {
                    npc.buffImmune[i] = true;
                }
            }
            npc.defense = npc.defDefense = _baseOverride.Defense ?? npc.defense;
            npc.noGravity = _baseOverride.HasNoGravity ?? npc.noGravity;
            npc.noTileCollide = _baseOverride.HasNoCollision ?? npc.noTileCollide;
            npc.boss = _baseOverride.IsBoss ?? npc.boss;
            npc.immortal = _baseOverride.IsImmortal ?? npc.immortal;
            npc.lavaImmune = _baseOverride.IsImmuneToLava ?? npc.lavaImmune;
            npc.trapImmune = _baseOverride.IsTrapImmune ?? npc.trapImmune;
            // Don't set npc.lifeMax so that the correct life is always sent to clients.
            npc.knockBackResist = _baseOverride.KnockbackMultiplier ?? npc.knockBackResist;
            npc.life = _baseOverride.MaxHp ?? npc.life;
            npc._givenName = _baseOverride.Name ?? npc._givenName;
            npc.npcSlots = _baseOverride.NpcSlots ?? npc.npcSlots;
            npc.value = _baseOverride.Value ?? npc.value;
        }

        /// <summary>
        ///     Loads the Lua definition, if possible.
        /// </summary>
        public void LoadLuaDefinition()
        {
            if (LuaPath == null)
            {
                return;
            }

            var lua = new Lua();
            lua.LoadCLRPackage();
            lua.DoString("import('System')");
            lua.DoString("import('OTAPI', 'Microsoft.Xna.Framework')");
            lua.DoString("import('OTAPI', 'Terraria')");
            lua.DoString("import('TShock', 'TShockAPI')");
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(NpcFunctions));
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(ProjectileFunctions));
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(TileFunctions));
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(PlayerFunctions));
			LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(PlayerCommandFunctions));

			lua["TileSize"] = TileFunctions.TileSize;
			lua["HalfTileSize"] = TileFunctions.HalfTileSize;
			lua["Center"] = new CenterOffsetHelper();

			//lua["_CustomName"] = _baseOverride.Name;
			//lua["_Name"] = "???"; //Lang.GetNPCNameValue();

			//lua["__npcNameContainer"] = new NpcNameContainer("???", _baseOverride.Name);
			lua["__npcNameContainer"] = new NpcNameContainer("???", Name );

			lua.DoFile(Path.Combine("npcs", LuaPath));
            _lua = lua;

			OnAiUpdate =		_lua.GetSafeFunction("OnAiUpdate");
			OnCheckReplace =	_lua.GetSafeFunction("OnCheckReplace");
            OnCheckSpawn =		_lua.GetSafeFunction("OnCheckSpawn");
            OnCollision =		_lua.GetSafeFunction("OnCollision");
			OnTileCollision =	_lua.GetSafeFunction("OnTileCollision");
			OnKilled =			_lua.GetSafeFunction("OnKilled");
            OnSpawn =			_lua.GetSafeFunction("OnSpawn");
            OnStrike =			_lua.GetSafeFunction("OnStrike");
			OnTransformed =		_lua.GetSafeFunction("OnTransformed");
        }

		internal void LinkBooModule(Assembly ass)
		{
			var moduleName = $"{Path.GetFileNameWithoutExtension(BooPath)}Module";
			var moduleType = ass.DefinedTypes.Where( dt => dt.Name == moduleName).FirstOrDefault();

			if(moduleType!=null)
			{
				var methods = moduleType.GetMethods(BindingFlags.Static|BindingFlags.Public);

				BooCheckReplace = methods.FindByName("OnCheckReplace")?.TryCreateDelegate<CheckReplaceHandler>();
				BooCheckSpawn = methods.FindByName("OnCheckSpawn")?.TryCreateDelegate<CheckSpawnHandler>();
				BooSpawn = methods.FindByName("OnSpawn")?.TryCreateDelegate<SpawnHandler>();
				BooCollision = methods.FindByName("OnCollision")?.TryCreateDelegate<CollisionHandler>();
				BooTileCollision = methods.FindByName("OnTileCollision")?.TryCreateDelegate<TileCollisionHandler>();
				BooTransformed = methods.FindByName("OnTransformed").TryCreateDelegate<TransformedHandler>();
				BooKilled = methods.FindByName("OnKilled").TryCreateDelegate<KilledHandler>();
				BooStrike = methods.FindByName("OnStrike").TryCreateDelegate<StrikeHandler>();
				BooAiUpdate = methods.FindByName("OnAiUpdate").TryCreateDelegate<AiUpdateHandler>();

				//var res = BooSpawn(null);

				//if( res )
				//	Debug.Print($"res={res}");
			}
		}
				
		internal void ThrowIfInvalid()
        {
            if (Name == null)
            {
                throw new FormatException($"{nameof(Name)} is null.");
            }
            if (int.TryParse(Name, out _))
            {
                throw new FormatException($"{nameof(Name)} cannot be a number.");
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new FormatException($"{nameof(Name)} is whitespace.");
            }
            if (BaseType < -65)
            {
                throw new FormatException($"{nameof(BaseType)} is too small.");
            }
            if (BaseType >= Main.maxNPCTypes)
            {
                throw new FormatException($"{nameof(BaseType)} is too large.");
            }
            if (LuaPath != null && !File.Exists(Path.Combine("npcs", LuaPath)))
            {
                throw new FormatException($"{nameof(LuaPath)} points to an invalid Lua file.");
            }
            if (_loot == null)
            {
                throw new FormatException("Loot is null.");
            }
            _loot.ThrowIfInvalid();
            if (_spawning == null)
            {
                throw new FormatException("Spawning is null.");
            }
            if (_baseOverride == null)
            {
                throw new FormatException("BaseOverride is null.");
            }
            _baseOverride.ThrowIfInvalid();
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal sealed class BaseOverrideDefinition
        {
            [JsonProperty]
            public int? AiStyle { get; private set; }

            [JsonProperty]
            public int[] BuffImmunities { get; private set; }

            [JsonProperty]
            public int? Defense { get; private set; }

            [JsonProperty]
            public bool? HasNoCollision { get; private set; }

            [JsonProperty]
            public bool? HasNoGravity { get; private set; }

            [JsonProperty]
            public bool? IsBoss { get; private set; }

            [JsonProperty]
            public bool? IsImmortal { get; private set; }

            [JsonProperty]
            public bool? IsImmuneToLava { get; private set; }

            [JsonProperty]
            public bool? IsTrapImmune { get; private set; }

            [JsonProperty]
            public float? KnockbackMultiplier { get; private set; }

            [JsonProperty]
            public int? MaxHp { get; private set; }

            [JsonProperty]
            public string Name { get; private set; }

            [JsonProperty]
            public float? NpcSlots { get; private set; }

            [JsonProperty]
            public float? Value { get; private set; }
			
            internal void ThrowIfInvalid()
            {
                if (BuffImmunities != null && BuffImmunities.Any(i => i <= 0 || i >= Main.maxBuffTypes))
                {
                    throw new FormatException($"{nameof(BuffImmunities)} must contain valid buff types.");
                }
                if (KnockbackMultiplier < 0)
                {
                    throw new FormatException($"{nameof(KnockbackMultiplier)} must be non-negative.");
                }
                if (MaxHp < 0)
                {
                    throw new FormatException($"{nameof(MaxHp)} must be non-negative.");
                }
                if (Value < 0)
                {
                    throw new FormatException($"{nameof(Value)} must be non-negative.");
                }
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class LootDefinition
        {
            [JsonProperty(Order = 2)]
            public List<LootEntryDefinition> Entries { get; private set; } = new List<LootEntryDefinition>();

            [JsonProperty(Order = 1)]
            public bool IsOverride { get; private set; }

            [JsonProperty(Order = 0)]
            public bool TallyKills { get; private set; }

            internal void ThrowIfInvalid()
            {
                if (Entries == null)
                {
                    throw new FormatException($"{nameof(Entries)} is null.");
                }
                foreach (var entry in Entries)
                {
                    entry.ThrowIfInvalid();
                }
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class SpawningDefinition
        {
            [JsonProperty(Order = 1)]
            public bool ShouldReplace { get; private set; }

            [JsonProperty(Order = 0)]
            public bool ShouldSpawn { get; private set; }

			[JsonProperty(Order = 2)]
			public int? SpawnRateOverride { get; private set; }
		}
    }
}
