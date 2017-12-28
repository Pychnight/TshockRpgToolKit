using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		[NotNull]
		public string Name { get; private set; } = "example";

		/// <summary>
		///     Gets the Lua path.
		/// </summary>
		[JsonProperty(Order = 1)]
        [CanBeNull]
        public string LuaPath { get; private set; }
		
        /// <summary>
        ///     Gets the NPC point values.
        /// </summary>
        [JsonProperty(Order = 2)]
        [NotNull]
        public Dictionary<string, int> NpcPointValues { get; private set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the completed message.
		/// </summary>
		[JsonProperty(Order = 3)]
		[NotNull]
		public string CompletedMessage { get; private set; } = "The example invasion has ended!";

		/// <summary>
		///     Gets a value indicating whether the invasion should occur at spawn only.
		/// </summary>
		[JsonProperty(Order = 4)]
		public bool AtSpawnOnly { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the invasion should scale by the number of players.
		/// </summary>
		[JsonProperty(Order = 5)]
        public bool ScaleByPlayers { get; private set; }

        /// <summary>
        ///     Gets the waves.
        /// </summary>
        [ItemNotNull]
        [JsonProperty(Order = 6)]
        [NotNull]
        public List<WaveDefinition> Waves { get; set; } = new List<WaveDefinition>();

		/// <summary>
		///     Gets a function that is invoked when the invasion is started.
		/// </summary>
		[CanBeNull]
		public LuaFunction OnInvasionStart { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		[CanBeNull]
		public LuaFunction OnInvasionEnd { get; private set; }
		
		/// <summary>
		///     Gets a function that is invoked when the invasion is updated.
		/// </summary>
		[CanBeNull]
		public LuaFunction OnUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is started.
		/// </summary>
		[CanBeNull]
		public LuaFunction OnWaveStart { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		[CanBeNull]
		public LuaFunction OnWaveEnd { get; private set; }

		/// <summary>
		///     Disposes the definition.
		/// </summary>
		public void Dispose()
        {
			OnInvasionStart = null;
			OnInvasionEnd = null;
            OnUpdate = null;
			OnWaveStart = null;
			OnWaveEnd = null;
            _lua?.Dispose();
            _lua = null;
        }

        /// <summary>
        ///     Loads the Lua definition.
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
            lua.DoFile(Path.Combine("npcs", LuaPath));
            _lua = lua;

			OnInvasionStart = _lua["OnInvasionStart"] as LuaFunction;
			OnInvasionEnd = _lua["OnInvasionEnd"] as LuaFunction;
			OnUpdate = _lua["OnUpdate"] as LuaFunction;
			OnWaveStart = _lua["OnWaveStart"] as LuaFunction;
			OnWaveEnd = _lua["OnWaveEnd"] as LuaFunction;
        }

        internal void ThrowIfInvalid()
        {
            if (Name == null)
            {
                throw new FormatException($"{nameof(Name)} is null.");
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new FormatException($"{nameof(Name)} is whitespace.");
            }
            if (LuaPath != null && !File.Exists(Path.Combine("npcs", LuaPath)))
            {
                throw new FormatException($"{nameof(LuaPath)} points to an invalid Lua file.");
            }
            if (NpcPointValues == null)
            {
                throw new FormatException($"{nameof(NpcPointValues)} is null.");
            }
            if (NpcPointValues.Count == 0)
            {
                throw new FormatException($"{nameof(NpcPointValues)} must not be empty.");
            }
            if (NpcPointValues.Any(kvp => kvp.Value <= 0))
            {
                throw new FormatException($"{nameof(NpcPointValues)} must contain positive values.");
            }
            if (CompletedMessage == null)
            {
                throw new FormatException($"{nameof(CompletedMessage)} is null.");
            }
            if (Waves == null)
            {
                throw new FormatException($"{nameof(Waves)} is null.");
            }
            if (Waves.Count == 0)
            {
                throw new FormatException($"{nameof(Waves)} must not be empty.");
            }
            foreach (var wave in Waves)
            {
                wave.ThrowIfInvalid();
            }
        }
    }
}
