using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomNpcs.Npcs;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Diagnostics;
using System.Reflection;
using BooTS;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion manager. This class is a singleton.
    /// </summary>
    public sealed class InvasionManager : IDisposable
    {
		internal static readonly string InvasionsBasePath = "npcs";
        internal static readonly string InvasionsConfigPath = Path.Combine(InvasionsBasePath, "invasions.json");
        private static readonly Color InvasionTextColor = new Color(175, 25, 255);
		       
        private readonly CustomNpcsPlugin _plugin;
        private readonly Random _random = new Random();

        private string _currentMiniboss;
        private int _currentPoints;
        private int _currentWaveIndex;
        private List<InvasionDefinition> _definitions = new List<InvasionDefinition>();
        private DateTime _lastProgressUpdate;
        private int _requiredPoints;
				
		private Assembly invasionScriptsAssembly;
				
        internal InvasionManager(CustomNpcsPlugin plugin)
        {
            _plugin = plugin;

			LoadDefinitions();
			
            GeneralHooks.ReloadEvent += OnReload;
            // Register OnGameUpdate with priority 1 to guarantee that InvasionManager runs before NpcManager.
            ServerApi.Hooks.GameUpdate.Register(_plugin, OnGameUpdate, 1);
            ServerApi.Hooks.NpcKilled.Register(_plugin, OnNpcKilled);
        }

        /// <summary>
        ///     Gets the invasion manager instance.
        /// </summary>
        [CanBeNull]
        public static InvasionManager Instance { get; internal set; }

        /// <summary>
        ///     Gets the current invasion, or <c>null</c> if there is none.
        /// </summary>
        public InvasionDefinition CurrentInvasion { get; private set; }

        /// <summary>
        ///     Disposes the invasion manager.
        /// </summary>
        public void Dispose()
        {
            //File.WriteAllText(InvasionsConfigPath, JsonConvert.SerializeObject(_definitions, Formatting.Indented));

            GeneralHooks.ReloadEvent -= OnReload;
            ServerApi.Hooks.GameUpdate.Deregister(_plugin, OnGameUpdate);
            ServerApi.Hooks.NpcKilled.Deregister(_plugin, OnNpcKilled);

            CurrentInvasion = null;
            foreach (var definition in _definitions)
            {
                definition.Dispose();
            }
            _definitions.Clear();
        }

        /// <summary>
        ///     Finds the definition with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The definition, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [CanBeNull]
        public InvasionDefinition FindDefinition([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

			return _definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Starts the specified invasion.
        /// </summary>
        /// <param name="invasion">The invasion, or <c>null</c> to stop the current invasion.</param>
        public void StartInvasion([CanBeNull] InvasionDefinition invasion)
        {
			EndInvasion();

			CurrentInvasion = invasion;
            if (CurrentInvasion != null)
            {
				try
				{
					invasion.OnInvasionStart?.Invoke();
				}
				catch(Exception ex)
				{
					Utils.LogScriptRuntimeError(ex);
					invasion.OnInvasionStart = null;
				}
				
				CurrentInvasion.HasStarted = true;
				_currentWaveIndex = 0;
                StartCurrentWave();
            }
        }

		public void EndInvasion()
		{
			if(CurrentInvasion!=null)
			{
				TryEndPreviousWave();
								
				try
				{
					CurrentInvasion.OnInvasionEnd?.Invoke();
				}
				catch( Exception ex )
				{
					Utils.LogScriptRuntimeError(ex);
					CurrentInvasion.OnInvasionEnd = null;
				}
				
				TSPlayer.All.SendMessage(CurrentInvasion.CompletedMessage, new Color(175, 75, 225));
				CurrentInvasion.HasStarted = false;//...probably not needed
				CurrentInvasion = null;
			}
		}
		
		private IEnumerable<EnsuredMethodSignature> getEnsuredMethodSignatures()
		{
			var sigs = new List<EnsuredMethodSignature>()
			{
				new EnsuredMethodSignature("OnWaveStart")
					.AddParameter("waveIndex",typeof(int))
					.AddParameter("waveDefinition",typeof(WaveDefinition)),

				new EnsuredMethodSignature("OnWaveEnd")
					.AddParameter("waveIndex",typeof(int))
					.AddParameter("waveDefinition",typeof(WaveDefinition)),

				new EnsuredMethodSignature("OnWaveUpdate")
					.AddParameter("waveIndex",typeof(int))
					.AddParameter("waveDefinition",typeof(WaveDefinition))
					.AddParameter("currentPoints",typeof(int)),
			};

			return sigs;
		}
		
		private void LoadDefinitions()
		{
			_definitions = DefinitionLoader.LoadFromFile<InvasionDefinition>(InvasionsConfigPath);

			//get script files paths
			var booScripts = _definitions.Where(d => !string.IsNullOrWhiteSpace(d.ScriptPath))
										 .Select( d => Path.Combine(InvasionsBasePath, d.ScriptPath))
										 .ToList();

			if( booScripts.Count > 0 )
			{
				//Debug.Print($"Compiling boo invasion scripts.");
				CustomNpcsPlugin.Instance.LogPrint($"Compiling invasion scripts.", TraceLevel.Info);
				var context = BooScriptCompiler.Compile("ScriptedInvasions.dll",
															booScripts,
															ScriptHelpers.GetReferences(),
															ScriptHelpers.GetDefaultImports(),
															getEnsuredMethodSignatures());

				CustomNpcsPlugin.Instance.LogPrintBooErrors(context);

				if(context.Errors.Count<1)
					CustomNpcsPlugin.Instance.LogPrintBooWarnings(context);

				invasionScriptsAssembly = context.GeneratedAssembly;

				if( invasionScriptsAssembly != null )
				{
					//Debug.Print($"Compilation succeeded.");
					CustomNpcsPlugin.Instance.LogPrint($"Success.", TraceLevel.Info);

					foreach( var d in _definitions )
						d.LinkToScript(invasionScriptsAssembly);
				}
				else
				{
					//Debug.Print($"Compilation failed.");
					CustomNpcsPlugin.Instance.LogPrint($"Failed.", TraceLevel.Info);
				}
			}
		}

		private void NotifyRelevantPlayers()
        {
            foreach (var player in TShock.Players.Where(p => p != null && p.Active && ShouldSpawnInvasionNpcs(p)))
            {
                player.SendData(PacketTypes.ReportInvasionProgress, "", _currentPoints, _requiredPoints, 0,
                    _currentWaveIndex + 1);
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (CurrentInvasion == null || CurrentInvasion.HasStarted == false)
            {
                return;
            }
						
			var activePlayers = TShock.Players.Where(p => p?.Active == true);

			if(activePlayers.Count()<1)
			{
				CustomNpcsPlugin.Instance.LogPrint("There no more active players, ending the current invasion.", TraceLevel.Info);
				EndInvasion();
				return;
			}

            Utils.TrySpawnForEachPlayer(TrySpawnInvasionNpc);

			// Prevent other NPCs from spawning for relevant players.
			foreach( var player in activePlayers )
            {
				if(ShouldSpawnInvasionNpcs(player))
					player.TPlayer.activeNPCs = 10000;
            }

            if (_currentPoints >= _requiredPoints && _currentMiniboss == null)
            {
				if (++_currentWaveIndex == CurrentInvasion.Waves.Count)
                {
					EndInvasion();
                    return;
                }
				else
				{
					TryEndPreviousWave();
					StartCurrentWave();
				}
            }

            var now = DateTime.UtcNow;
            if (now - _lastProgressUpdate > TimeSpan.FromSeconds(1))
            {
                NotifyRelevantPlayers();
                _lastProgressUpdate = now;
            }

			try
			{
				CurrentInvasion?.OnUpdate?.Invoke();
			}
			catch( Exception ex )
			{
				Utils.LogScriptRuntimeError(ex);
				CurrentInvasion.OnUpdate = null;
			}
		}

		private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (CurrentInvasion == null)
            {
                return;
            }

            var npc = args.npc;
            var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
            var npcNameOrType = customNpc?.Definition.Name ?? npc.netID.ToString();
            if (npcNameOrType.Equals(_currentMiniboss, StringComparison.OrdinalIgnoreCase))
            {
				try
				{
					CurrentInvasion.OnBossDefeated?.Invoke();
				}
				catch(Exception ex)
				{
					Utils.LogScriptRuntimeError(ex);
					CurrentInvasion.OnBossDefeated = null;
					_currentMiniboss = null;
				}
	        }
            else if (CurrentInvasion.NpcPointValues.TryGetValue(npcNameOrType, out var points))
            {
                _currentPoints += points;
                _currentPoints = Math.Min(_currentPoints, _requiredPoints);
                NotifyRelevantPlayers();

				if(_currentWaveIndex>=0 && _currentWaveIndex<CurrentInvasion.Waves.Count )
				{
					var wave = CurrentInvasion.Waves[_currentWaveIndex];
					if( wave != null )
					{
						try
						{
							CurrentInvasion.OnWaveUpdate?.Invoke(_currentWaveIndex, wave, _currentPoints);
						}
						catch( Exception ex )
						{
							Utils.LogScriptRuntimeError(ex);
							CurrentInvasion.OnWaveUpdate = null;
						}
					}
				}
			}
        }

        private void OnReload(ReloadEventArgs args)
        {
            CurrentInvasion = null;
            
			foreach( var definition in _definitions )
			{
				definition.Dispose();
			}
			_definitions.Clear();

			LoadDefinitions();

			args.Player.SendSuccessMessage("[CustomNpcs] Reloaded invasions!");
        }

        private bool ShouldSpawnInvasionNpcs(TSPlayer player)
        {
            var playerPosition = player.TPlayer.position;
            return !CurrentInvasion.AtSpawnOnly || Main.spawnTileX * 16.0 - 3000 < playerPosition.X &&
                   playerPosition.X < Main.spawnTileX * 16.0 + 3000 &&
                   playerPosition.Y < Main.worldSurface * 16.0 + NPC.sHeight;
        }

        private void StartCurrentWave()
        {
			var wave = CurrentInvasion.Waves[_currentWaveIndex];
			TSPlayer.All.SendMessage(wave.StartMessage, InvasionTextColor);
			_currentPoints = 0;
			_currentMiniboss = wave.Miniboss;
			_requiredPoints = wave.PointsRequired * ( CurrentInvasion.ScaleByPlayers ? TShock.Utils.ActivePlayers() : 1 );

			if(wave!=null)
			{
				try
				{
					CurrentInvasion.OnWaveStart?.Invoke(_currentWaveIndex, wave);
				}
				catch( Exception ex )
				{
					Utils.LogScriptRuntimeError(ex);
					CurrentInvasion.OnWaveStart = null;
				}
			}
		}

		private void TryEndPreviousWave()
		{
			//run end event for previous wave, if there was a previous wave
			var previousWaveIndex = _currentWaveIndex - 1;
			if( previousWaveIndex >= 0 )
			{
				var previousWave = CurrentInvasion.Waves[previousWaveIndex];

				try
				{
					CurrentInvasion.OnWaveEnd?.Invoke(previousWaveIndex, previousWave);
				}
				catch( Exception ex )
				{
					Utils.LogScriptRuntimeError(ex);
					CurrentInvasion.OnWaveEnd = null;
				}
			}
		}

        private void TrySpawnInvasionNpc(TSPlayer player, int tileX, int tileY)
        {
            if (!ShouldSpawnInvasionNpcs(player))
            {
                return;
            }

            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            if (player.TPlayer.activeNPCs >= currentWave.MaxSpawns || _random.Next(currentWave.SpawnRate) != 0)
            {
                return;
            }

            if (_currentPoints >= _requiredPoints && _currentMiniboss != null)
            {
                foreach (var npc in Main.npc.Where(n => n?.active == true))
                {
                    var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
                    var npcNameOrType = customNpc?.Definition.Name ?? npc.netID.ToString();
                    if (npcNameOrType.Equals(_currentMiniboss, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                Utils.SpawnVanillaOrCustomNpc(_currentMiniboss, tileX, tileY);
            }
            else
            {
                var randomNpcNameOrType = Utils.PickRandomWeightedKey(currentWave.NpcWeights);
                Utils.SpawnVanillaOrCustomNpc(randomNpcNameOrType, tileX, tileY);
            }
        }
    }
}
