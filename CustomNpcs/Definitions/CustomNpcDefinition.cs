using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua;
using Terraria;

namespace CustomNpcs.Definitions
{
    /// <summary>
    ///     Represents a custom NPC definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CustomNpcDefinition : IDisposable
    {
        [JsonProperty("BaseOverride", Order = 2)]
        private BaseOverrideDefinition _baseOverride = new BaseOverrideDefinition();

        [JsonProperty("Loot", Order = 3)]
        private LootDefinition _loot = new LootDefinition();

        private Lua _lua;

        [JsonProperty("Spawning", Order = 4)]
        private SpawningDefinition _spawning = new SpawningDefinition();

        /// <summary>
        ///     Gets the base type.
        /// </summary>
        [JsonProperty(Order = 1)]
        public int BaseType { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the NPC should have custom spawn.
        /// </summary>
        public bool CustomSpawn => _spawning.CustomSpawn;

        /// <summary>
        ///     Gets the spawn rate multiplier.
        /// </summary>
        [CanBeNull]
        public double? CustomSpawnRateMultiplier => _spawning.CustomSpawnRateMultiplier;

        /// <summary>
        ///     Gets the loot entries.
        /// </summary>
        [ItemNotNull]
        public List<LootEntryDefinition> LootEntries => _loot.Entries;

        /// <summary>
        ///     Gets a value indicating whether to override loot.
        /// </summary>
        public bool LootOverride => _loot.Override;

        /// <summary>
        ///     Gets the internal name.
        /// </summary>
        [JsonProperty(Order = 0)]
        [CanBeNull]
        public string Name { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC AI is updated.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnAiUpdate { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC is checked for spawning.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnCheckSpawn { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC collides with a player.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnCollision { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when NPC is killed.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnKilled { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC is spawned.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnSpawn { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC is struck.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnStrike { get; private set; }

        /// <summary>
        ///     Gets the replacement chance.
        /// </summary>
        [CanBeNull]
        public double? ReplacementChance => _spawning.ReplacementChance;

        /// <summary>
        ///     Gets the replacement target type.
        /// </summary>
        [CanBeNull]
        public int? ReplacementTargetType => _spawning.ReplacementTargetType;

        /// <summary>
        ///     Gets a value indicating whether the NPC should aggressively update due to unsynced changes with clients.
        /// </summary>
        public bool ShouldAggressivelyUpdate => _baseOverride.AiStyle != null || _baseOverride.HasNoGravity != null;

        /// <summary>
        ///     Gets a value indicating whether the NPC should update on hit.
        /// </summary>
        public bool ShouldUpdateOnHit => _baseOverride.Defense != null || _baseOverride.KnockbackMultiplier != null;

        /// <summary>
        ///     Disposes the definition.
        /// </summary>
        public void Dispose()
        {
            OnAiUpdate = null;
            OnCheckSpawn = null;
            OnCollision = null;
            OnKilled = null;
            OnSpawn = null;
            OnStrike = null;
            _lua?.Dispose();
            _lua = null;
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

            npc.aiStyle = _baseOverride.AiStyle ?? npc.aiStyle;
            npc.boss = _baseOverride.IsBoss ?? npc.boss;
            npc.defense = npc.defDefense = _baseOverride.Defense ?? npc.defense;
            // Don't set npc.lifeMax so that the correct life is always sent to clients.
            npc.life = _baseOverride.MaxHp ?? npc.life;
            npc.knockBackResist = _baseOverride.KnockbackMultiplier ?? npc.knockBackResist;
            npc._givenName = _baseOverride.Name ?? npc._givenName;
            npc.noGravity = _baseOverride.HasNoGravity ?? npc.noGravity;
            npc.npcSlots = _baseOverride.NpcSlots ?? npc.npcSlots;
            npc.value = _baseOverride.Value ?? npc.value;
        }

        /// <summary>
        ///     Loads the Lua definition, if possible.
        /// </summary>
        public void LoadLuaDefinition()
        {
            var luaPath = Path.Combine("npcs", $"{Name}.lua");
            if (!File.Exists(luaPath))
            {
                return;
            }

            var lua = new Lua();
            lua.LoadCLRPackage();
            lua.DoString("import('System')");
            lua.DoString("import('OTAPI', 'Microsoft.Xna.Framework')");
            lua.DoString("import('OTAPI', 'Terraria')");
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(NpcFunctions));
            lua.DoFile(luaPath);
            _lua = lua;

            OnAiUpdate = _lua["OnAiUpdate"] as LuaFunction;
            OnCheckSpawn = _lua["OnCheckSpawn"] as LuaFunction;
            OnCollision = _lua["OnCollision"] as LuaFunction;
            OnKilled = _lua["OnKilled"] as LuaFunction;
            OnSpawn = _lua["OnSpawn"] as LuaFunction;
            OnStrike = _lua["OnStrike"] as LuaFunction;
        }

        [JsonObject]
        private sealed class BaseOverrideDefinition
        {
            public int? AiStyle { get; set; }
            public int? Defense { get; set; }
            public bool? HasNoGravity { get; set; }
            public bool? IsBoss { get; set; }
            public float? KnockbackMultiplier { get; set; }
            public int? MaxHp { get; set; }
            public string Name { get; set; }
            public float? NpcSlots { get; set; }
            public float? Value { get; set; }
        }

        [JsonObject]
        private sealed class LootDefinition
        {
            public List<LootEntryDefinition> Entries { get; set; }
            public bool Override { get; set; }
        }

        [JsonObject]
        private sealed class SpawningDefinition
        {
            public bool CustomSpawn { get; set; }
            public double? CustomSpawnRateMultiplier { get; set; }
            public double? ReplacementChance { get; set; }
            public int? ReplacementTargetType { get; set; }
        }
    }
}
