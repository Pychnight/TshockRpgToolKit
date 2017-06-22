using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class InvasionDefinition : IDisposable
    {
        private Lua _lua;

        /// <summary>
        ///     Gets the completed message.
        /// </summary>
        [NotNull]
        [JsonProperty(Order = 5)]
        public string CompletedMessage { get; private set; } = "The example invasion has ended!";

        /// <summary>
        ///     Gets the custom NPC point values.
        /// </summary>
        [NotNull]
        [JsonProperty(Order = 4)]
        public Dictionary<string, int> CustomNpcPointValues { get; private set; } = new Dictionary<string, int>();

        /// <summary>
        ///     Gets the Lua path.
        /// </summary>
        [JsonProperty(Order = 1)]
        [NotNull]
        public string LuaPath { get; private set; } = "npcs\\invasions\\example.lua";

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [JsonProperty(Order = 0)]
        [NotNull]
        public string Name { get; private set; } = "example";

        /// <summary>
        ///     Gets the NPC point values.
        /// </summary>
        [JsonProperty(Order = 3)]
        public Dictionary<int, int> NpcPointValues { get; private set; } = new Dictionary<int, int>();

        /// <summary>
        ///     Gets a function that is invoked when the invasion is updated.
        /// </summary>
        [CanBeNull]
        public LuaFunction OnUpdate { get; private set; }

        /// <summary>
        ///     Gets the waves.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        [JsonProperty(Order = 2)]
        public List<WaveDefinition> Waves { get; set; } = new List<WaveDefinition>();

        /// <summary>
        ///     Disposes the invasion definition.
        /// </summary>
        public void Dispose()
        {
            OnUpdate = null;
            _lua?.Dispose();
            _lua = null;
        }

        /// <summary>
        ///     Loads the Lua definition.
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
            lua.DoString("import('TShock', 'TShockAPI')");
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(NpcFunctions));
            lua.DoFile(luaPath);
            _lua = lua;

            OnUpdate = _lua["OnUpdate"] as LuaFunction;
        }
    }
}
