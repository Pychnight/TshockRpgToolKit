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
        ///     Gets or sets a function that is invoked when the NPC AI is updated.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnAiUpdate { get; set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when NPC is killed.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnKilled { get; set; }

        /// <summary>
        ///     Gets or sets a function that is invoked when the NPC is struck.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnStrike { get; set; }

        /// <summary>
        ///     Gets the replacement chance.
        /// </summary>
        [JsonProperty]
        public double? ReplacementChance { get; private set; }

        /// <summary>
        ///     Gets the value override.
        /// </summary>
        [CanBeNull]
        [JsonProperty]
        public int? ValueOverride { get; private set; }

        /// <summary>
        ///     Disposes the definition.
        /// </summary>
        public void Dispose()
        {
            Lua?.Dispose();
            Lua = null;
            OnAiUpdate = null;
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
            npc.value = ValueOverride ?? npc.value;
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
            OnKilled = Lua["OnKilled"] as LuaFunction;
            OnStrike = Lua["OnStrike"] as LuaFunction;
        }
    }
}
