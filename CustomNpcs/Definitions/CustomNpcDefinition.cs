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
        /// <summary>
        ///     Gets the AI style override.
        /// </summary>
        [JsonProperty]
        public int? AiStyleOverride { get; private set; }

        /// <summary>
        ///     Gets the base type.
        /// </summary>
        [JsonProperty]
        public int BaseType { get; private set; } = 1;

        /// <summary>
        ///     Gets the defense override.
        /// </summary>
        [JsonProperty]
        public int? DefenseOverride { get; private set; }

        /// <summary>
        ///     Gets the loot override.
        /// </summary>
        [CanBeNull]
        [ItemNotNull]
        [JsonProperty]
        public List<LootDefinition> LootOverride { get; private set; } =
            new List<LootDefinition> {new LootDefinition()};

        /// <summary>
        ///     Gets the Lua instance.
        /// </summary>
        [CanBeNull]
        public Lua Lua { get; private set; }

        /// <summary>
        ///     Gets the maximum HP override.
        /// </summary>
        [JsonProperty]
        public int? MaxHpOverride { get; private set; }

        /// <summary>
        ///     Gets the internal name.
        /// </summary>
        [JsonProperty]
        [NotNull]
        public string Name { get; private set; } = "example";

        /// <summary>
        ///     Gets the name override.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public string NameOverride { get; private set; }

        /// <summary>
        ///     Gets the NPC slots override.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public double? NpcSlotsOverride { get; private set; }

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
        ///     Gets or sets a function that is invoked when NPC is killed.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnKilled { get; private set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC is struck.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnStrike { get; private set; }

        /// <summary>
        ///     Gets the replacement chance.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public double? ReplacementChance { get; private set; }

        /// <summary>
        /// Gets the replacement target type.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public int? ReplacementTargetType { get; private set; }

        /// <summary>
        ///     Gets the spawn rate multiplier.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public double? SpawnRateMultiplier { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the NPC spawns naturally.
        /// </summary>
        [JsonProperty]
        public bool SpawnsNaturally { get; private set; } = true;

        /// <summary>
        ///     Gets the value override.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public double? ValueOverride { get; private set; }

        /// <summary>
        ///     Disposes the definition.
        /// </summary>
        public void Dispose()
        {
            Lua?.Dispose();
            Lua = null;
            OnAiUpdate = null;
            OnCheckSpawn = null;
            OnKilled = null;
            OnStrike = null;
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

            npc.aiStyle = AiStyleOverride ?? npc.aiStyle;
            npc.defense = npc.defDefense = DefenseOverride ?? npc.defense;
            // Don't set npc.lifeMax. This way, whenever packet 23 is sent, the correct life is always sent.
            npc.life = MaxHpOverride ?? npc.life;
            npc._givenName = NameOverride ?? npc._givenName;
            // Set npcSlots to 0 if this is not a replacement, as we don't want custom NPCs to interfere with normal NPC
            // spawning.
            npc.npcSlots = ReplacementChance == null ? 0 : (float)(NpcSlotsOverride ?? npc.npcSlots);
            npc.value = (float)(ValueOverride ?? npc.value);
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
            Lua = lua;

            OnAiUpdate = Lua["OnAiUpdate"] as LuaFunction;
            OnCheckSpawn = Lua["OnCheckSpawn"] as LuaFunction;
            OnKilled = Lua["OnKilled"] as LuaFunction;
            OnStrike = Lua["OnStrike"] as LuaFunction;
        }
    }
}
