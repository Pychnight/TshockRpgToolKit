using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua;
using Terraria;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents a custom NPC definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CustomNpcDefinition : IDisposable
    {
        [JsonProperty("BaseOverride", Order = 3)]
        private BaseOverrideDefinition _baseOverride = new BaseOverrideDefinition();

        [JsonProperty("Loot", Order = 4)]
        private LootDefinition _loot = new LootDefinition();

        private Lua _lua;

        [JsonProperty("Spawning", Order = 5)]
        private SpawningDefinition _spawning = new SpawningDefinition();

        /// <summary>
        ///     Gets the base type.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int BaseType { get; private set; }

        /// <summary>
        ///     Gets the custom spawn weight.
        /// </summary>
        [CanBeNull]
        public int? CustomSpawnWeight => _spawning.SpawnWeight;

        /// <summary>
        ///     Gets the loot entries.
        /// </summary>
        [ItemNotNull]
        public List<LootEntryDefinition> LootEntries => _loot.Entries;

        /// <summary>
        ///     Gets a value indicating whether to override loot.
        /// </summary>
        public bool LootOverride => _loot.IsOverride;

        /// <summary>
        ///     Gets the Lua path.
        /// </summary>
        [JsonProperty(Order = 1)]
        [NotNull]
        public string LuaPath { get; private set; } = "npcs\\example.lua";

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
        public LuaFunction OnAiUpdate { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is checked for spawning.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnCheckSpawn { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC collides with a player.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnCollision { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when NPC is killed.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnKilled { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is spawned.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnSpawn { get; private set; }

        /// <summary>
        ///     Gets a function that is invoked when the NPC is struck.
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
        public bool ShouldAggressivelyUpdate =>
            _baseOverride.AiStyle != null || _baseOverride.HasNoCollision != null || _baseOverride.HasNoGravity != null;

        /// <summary>
        ///     Gets a value indicating whether the NPC should have custom spawn.
        /// </summary>
        public bool ShouldCustomSpawn => _spawning.ShouldSpawn;

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
            npc.noTileCollide = _baseOverride.HasNoCollision ?? npc.noTileCollide;
            npc.npcSlots = _baseOverride.NpcSlots ?? npc.npcSlots;
            npc.value = _baseOverride.Value ?? npc.value;
        }

        /// <summary>
        ///     Loads the Lua definition, if possible.
        /// </summary>
        public void LoadLuaDefinition()
        {
            var luaPath = Path.Combine("npcs", LuaPath);
            if (!File.Exists(luaPath))
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
            lua.DoFile(luaPath);
            _lua = lua;

            OnAiUpdate = _lua["OnAiUpdate"] as LuaFunction;
            OnCheckSpawn = _lua["OnCheckSpawn"] as LuaFunction;
            OnCollision = _lua["OnCollision"] as LuaFunction;
            OnKilled = _lua["OnKilled"] as LuaFunction;
            OnSpawn = _lua["OnSpawn"] as LuaFunction;
            OnStrike = _lua["OnStrike"] as LuaFunction;
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class BaseOverrideDefinition
        {
            [JsonProperty]
            public int? AiStyle { get; private set; }

            [JsonProperty]
            public int? Defense { get; private set; }

            [JsonProperty]
            public bool? HasNoCollision { get; private set; }

            [JsonProperty]
            public bool? HasNoGravity { get; private set; }

            [JsonProperty]
            public bool? IsBoss { get; private set; }

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
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class LootDefinition
        {
            [JsonProperty(Order = 1)]
            public List<LootEntryDefinition> Entries { get; private set; }

            [JsonProperty(Order = 0)]
            public bool IsOverride { get; private set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class SpawningDefinition
        {
            [JsonProperty(Order = 3)]
            public double? ReplacementChance { get; private set; }

            [JsonProperty(Order = 2)]
            public int? ReplacementTargetType { get; private set; }

            [JsonProperty(Order = 0)]
            public bool ShouldSpawn { get; private set; }

            [JsonProperty(Order = 1)]
            public int? SpawnWeight { get; private set; }
        }
    }
}
