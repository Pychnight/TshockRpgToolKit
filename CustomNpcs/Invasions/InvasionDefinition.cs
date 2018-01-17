using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class InvasionDefinition : IDisposable
    {
       	/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		[NotNull]
		public string Name { get; private set; } = "example";

		/// <summary>
		///     Gets the script path.
		/// </summary>
		[JsonProperty(Order = 1)]
        [CanBeNull]
        public string ScriptPath { get; private set; }
		
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
		///		Used to keep OnGameUpdate from firing events too early.
		/// </summary>
		public bool HasStarted { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is started.
		/// </summary>
		public InvasionStartHandler OnInvasionStart { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		public InvasionEndHandler OnInvasionEnd { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is updated.
		/// </summary>
		public InvasionUpdateHandler OnUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is started.
		/// </summary>
		public WaveStartHandler OnWaveStart { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		public WaveEndHandler OnWaveEnd { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		public WaveUpdateHandler OnWaveUpdate { get; private set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		public BossDefeatedHandler OnBossDefeated { get; private set; }

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
			OnWaveUpdate = null;
			OnBossDefeated = null;
        }

		//      /// <summary>
		//      ///     Loads the Lua definition.
		//      /// </summary>
		//      public void LoadLuaDefinition()
		//      {
		//          if (ScriptPath == null)
		//          {
		//              return;
		//          }

		//          var lua = new Lua();
		//	lua.LoadCLRPackage();
		//	lua.DoString("import('System')");
		//	lua.DoString("import('OTAPI', 'Microsoft.Xna.Framework')");
		//	lua.DoString("import('OTAPI', 'Terraria')");
		//	lua.DoString("import('TShock', 'TShockAPI')");
		//	LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(NpcFunctions));
		//	lua.DoFile(Path.Combine("npcs", ScriptPath));
		//	_lua = lua;

		//	OnInvasionStart = _lua.GetSafeFunction("OnInvasionStart");
		//	OnInvasionEnd = _lua.GetSafeFunction("OnInvasionEnd");
		//	OnUpdate = _lua.GetSafeFunction("OnUpdate");
		//	OnWaveStart = _lua.GetSafeFunction("OnWaveStart");
		//	OnWaveEnd = _lua.GetSafeFunction("OnWaveEnd");
		//	OnWaveUpdate = _lua.GetSafeFunction("OnWaveUpdate");
		//	OnBossDefeated = _lua.GetSafeFunction("OnBossDefeated");
		//}

		internal bool LinkToScript(Assembly ass)
		{
			if( ass == null )
				return false;

			if( string.IsNullOrWhiteSpace(ScriptPath) )
				return false;

			var linker = new BooModuleLinker(ass, ScriptPath);
			
			OnInvasionStart = linker.TryCreateDelegate<InvasionStartHandler>("OnInvasionStart");
			OnInvasionEnd = linker.TryCreateDelegate<InvasionEndHandler>("OnInvasionEnd");
			OnUpdate = linker.TryCreateDelegate<InvasionUpdateHandler>("OnUpdate");
			OnWaveStart = linker.TryCreateDelegate<WaveStartHandler>("OnWaveStart");
			OnWaveEnd = linker.TryCreateDelegate<WaveEndHandler>("OnWaveEnd");
			OnWaveUpdate = linker.TryCreateDelegate<WaveUpdateHandler>("OnWaveUpdate");
			OnBossDefeated = linker.TryCreateDelegate<BossDefeatedHandler>("OnBossDefeated");

			return true;
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

			var rooted = Path.Combine(InvasionManager.InvasionsBasePath, ScriptPath);

			if (ScriptPath != null && !File.Exists(rooted))
            {
                throw new FormatException($"{nameof(ScriptPath)} points to an invalid script file.");
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
