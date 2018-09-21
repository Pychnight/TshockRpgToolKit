using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BooTS;
using Corruption.PluginSupport;
using Newtonsoft.Json;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class InvasionDefinition : DefinitionBase, IDisposable
    {
       	/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		public override string Name { get; protected internal set; } = "example";

		/// <summary>
		///     Gets the script path.
		/// </summary>
		[JsonProperty(Order = 1)]
        public override string ScriptPath { get; protected internal set; }
		
        /// <summary>
        ///     Gets the NPC point values.
        /// </summary>
        [JsonProperty(Order = 2)]
        public Dictionary<string, int> NpcPointValues { get; private set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the completed message.
		/// </summary>
		[JsonProperty(Order = 3)]
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
        [JsonProperty(Order = 6)]
        public List<WaveDefinition> Waves { get; set; } = new List<WaveDefinition>();

		/// <summary>
		///		Used to keep OnGameUpdate from firing events too early.
		/// </summary>
		public bool HasStarted { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is started.
		/// </summary>
		public InvasionStartHandler OnInvasionStart { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is ending.
		/// </summary>
		public InvasionEndHandler OnInvasionEnd { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the invasion is updated.
		/// </summary>
		public InvasionUpdateHandler OnUpdate { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the wave is started.
		/// </summary>
		public InvasionWaveStartHandler OnWaveStart { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the wave is ending.
		/// </summary>
		public InvasionWaveEndHandler OnWaveEnd { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the wave is updated.
		/// </summary>
		public InvasionWaveUpdateHandler OnWaveUpdate { get; internal set; }

		/// <summary>
		///     Gets a function that is invoked when the boss is defeated.
		/// </summary>
		public InvasionBossDefeatedHandler OnBossDefeated { get; internal set; }

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
		
		protected override bool OnLinkToScriptAssembly(Assembly ass)
		{
			if( ass == null )
				return false;

			if( string.IsNullOrWhiteSpace(ScriptPath) )
				return false;

			var linker = new BooModuleLinker(ass, ScriptPath);
			
			OnInvasionStart = linker.TryCreateDelegate<InvasionStartHandler>("OnInvasionStart");
			OnInvasionEnd = linker.TryCreateDelegate<InvasionEndHandler>("OnInvasionEnd");
			OnUpdate = linker.TryCreateDelegate<InvasionUpdateHandler>("OnUpdate");
			OnWaveStart = linker.TryCreateDelegate<InvasionWaveStartHandler>("OnWaveStart");
			OnWaveEnd = linker.TryCreateDelegate<InvasionWaveEndHandler>("OnWaveEnd");
			OnWaveUpdate = linker.TryCreateDelegate<InvasionWaveUpdateHandler>("OnWaveUpdate");
			OnBossDefeated = linker.TryCreateDelegate<InvasionBossDefeatedHandler>("OnBossDefeated");

			return true;
		}

		//protected internal override void ThrowIfInvalid()
  //      {
  //          if (Name == null)
  //          {
  //              throw new FormatException($"{nameof(Name)} is null.");
  //          }
  //          if (string.IsNullOrWhiteSpace(Name))
  //          {
  //              throw new FormatException($"{nameof(Name)} is whitespace.");
  //          }

		//	var rooted = Path.Combine(InvasionManager.InvasionsBasePath, ScriptPath);

		//	if (ScriptPath != null && !File.Exists(rooted))
  //          {
  //              throw new FormatException($"{nameof(ScriptPath)} points to an invalid script file.");
  //          }
  //          if (NpcPointValues == null)
  //          {
  //              throw new FormatException($"{nameof(NpcPointValues)} is null.");
  //          }
  //          if (NpcPointValues.Count == 0)
  //          {
  //              throw new FormatException($"{nameof(NpcPointValues)} must not be empty.");
  //          }
  //          if (NpcPointValues.Any(kvp => kvp.Value <= 0))
  //          {
  //              throw new FormatException($"{nameof(NpcPointValues)} must contain positive values.");
  //          }
  //          if (CompletedMessage == null)
  //          {
  //              throw new FormatException($"{nameof(CompletedMessage)} is null.");
  //          }
  //          if (Waves == null)
  //          {
  //              throw new FormatException($"{nameof(Waves)} is null.");
  //          }
  //          if (Waves.Count == 0)
  //          {
  //              throw new FormatException($"{nameof(Waves)} must not be empty.");
  //          }
  //          foreach (var wave in Waves)
  //          {
  //              wave.ThrowIfInvalid();
  //          }
  //      }

		protected override void OnValidate(ValidationResult result)
		{
			if( Name == null )
			{
				//throw new FormatException($"{nameof(Name)} is null.");
				result.AddError($"{nameof(Name)} is null.", FilePath, LineNumber, LinePosition);
			}
			if( string.IsNullOrWhiteSpace(Name) )
			{
				//throw new FormatException($"{nameof(Name)} is whitespace.");
				result.AddError($"{nameof(Name)} is whitespace.", FilePath, LineNumber, LinePosition);
			}

			//Disabling this check for rooted, because at the point this is ran, InvasionManager may not have been set yet, causing 
			//an NRE to get logged.
			//var rooted = Path.Combine(InvasionManager.Instance.BasePath, ScriptPath ?? "");

			//if( ScriptPath != null && !File.Exists(rooted) )
			//{
			//	//throw new FormatException($"{nameof(ScriptPath)} points to an invalid script file.");
			//	result.AddError($"{nameof(ScriptPath)} points to an invalid script file.", FilePath, LineNumber, LinePosition);
			//}
			if( NpcPointValues == null )
			{
				//throw new FormatException($"{nameof(NpcPointValues)} is null.");
				result.AddError($"{nameof(NpcPointValues)} is null.", FilePath, LineNumber, LinePosition);
			}
			if( NpcPointValues.Count == 0 )
			{
				//throw new FormatException($"{nameof(NpcPointValues)} must not be empty.");
				result.AddError($"{nameof(NpcPointValues)} must not be empty.", FilePath, LineNumber, LinePosition);
			}
			if( NpcPointValues.Any(kvp => kvp.Value <= 0) )
			{
				//throw new FormatException($"{nameof(NpcPointValues)} must contain positive values.");
				result.AddError($"{nameof(NpcPointValues)} must contain positive values.", FilePath, LineNumber, LinePosition);
			}
			if( CompletedMessage == null )
			{
				//throw new FormatException($"{nameof(CompletedMessage)} is null.");
				result.AddError($"{nameof(CompletedMessage)} is null.", FilePath, LineNumber, LinePosition);
			}
			if( Waves == null )
			{
				//throw new FormatException($"{nameof(Waves)} is null.");
				result.AddError($"{nameof(Waves)} is null.", FilePath, LineNumber, LinePosition);
			}
			if( Waves.Count == 0 )
			{
				//throw new FormatException($"{nameof(Waves)} must not be empty.");
				result.AddError($"{nameof(Waves)} must not be empty.", FilePath, LineNumber, LinePosition);
			}
			foreach( var wave in Waves )
			{
				//wave.ThrowIfInvalid();
				var res = wave.Validate();
				result.AddValidationResult(res);
			}
		}
	}
}
