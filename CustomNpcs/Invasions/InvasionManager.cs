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

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion manager. This class is a singleton.
    /// </summary>
    public sealed class InvasionManager : IDisposable
    {
        private static readonly string InvasionsPath = Path.Combine("npcs", "invasions.json");
        private static readonly Color InvasionTextColor = new Color(175, 25, 255);

        private readonly object _lock = new object();
        private readonly CustomNpcsPlugin _plugin;
        private readonly Random _random = new Random();

        private string _currentMiniboss;
        private int _currentPoints;
        private int _currentWaveIndex;
        private List<InvasionDefinition> _definitions = new List<InvasionDefinition>();
        private DateTime _lastProgressUpdate;
        private int _requiredPoints;
				
        internal InvasionManager(CustomNpcsPlugin plugin)
        {
            _plugin = plugin;

            Utils.TryExecuteLua(LoadDefinitions, "InvasionManager");

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
            File.WriteAllText(InvasionsPath, JsonConvert.SerializeObject(_definitions, Formatting.Indented));

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

            lock (_lock)
            {
                return _definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
            }
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
				lock(_lock )
				{
					var onInvasionStart = invasion.OnInvasionStart;
					if( onInvasionStart != null )
					{
						Utils.TryExecuteLua(() => onInvasionStart.Call(), invasion.Name);
					}
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

				lock( _lock )
				{
					var onInvasionEnd = CurrentInvasion.OnInvasionEnd;
					if( onInvasionEnd != null )
					{
						Utils.TryExecuteLua(() => onInvasionEnd.Call(), CurrentInvasion.Name);
					}
				}

				TSPlayer.All.SendMessage(CurrentInvasion.CompletedMessage, new Color(175, 75, 225));
				CurrentInvasion.HasStarted = false;//...probably not needed
				CurrentInvasion = null;
			}
		}

        private void LoadDefinitions()
        {
            if (File.Exists(InvasionsPath))
            {
                _definitions = JsonConvert.DeserializeObject<List<InvasionDefinition>>(File.ReadAllText(InvasionsPath));
                var failedDefinitions = new List<InvasionDefinition>();
                foreach (var definition in _definitions)
                {
                    try
                    {
                        definition.ThrowIfInvalid();
                    }
                    catch (FormatException ex)
                    {
                        TShock.Log.ConsoleError(
                            $"[CustomNpcs] An error occurred while parsing invasion '{definition.Name}': {ex.Message}");
                        failedDefinitions.Add(definition);
                        continue;
                    }

                    definition.LoadLuaDefinition();
                }
                _definitions = _definitions.Except(failedDefinitions).ToList();
            }
			else
			{
				TShock.Log.ConsoleError($"Invasions configuration does not exist. Expected config file to be at: {InvasionsPath}");
				_definitions = new List<InvasionDefinition>();
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

            Utils.TrySpawnForEachPlayer(TrySpawnInvasionNpc);
            // Prevent other NPCs from spawning for relevant players.
            foreach (var player in TShock.Players.Where(p => p?.Active == true && ShouldSpawnInvasionNpcs(p)))
            {
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

            var onUpdate = CurrentInvasion.OnUpdate;
            if (onUpdate != null)
            {
                lock (_lock)
                {
                    Utils.TryExecuteLua(() => onUpdate.Call(), CurrentInvasion.Name);
                }
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
				lock(_lock)
				{
					var onBossDefeated = CurrentInvasion.OnBossDefeated;
					if( onBossDefeated != null )
					{
						Utils.TryExecuteLua(() => onBossDefeated.Call(), CurrentInvasion.Name);
					}
				}

                _currentMiniboss = null;
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
						lock( _lock )
						{
							var onWaveUpdate = CurrentInvasion.OnWaveUpdate;
							if( onWaveUpdate != null )
							{
								Utils.TryExecuteLua(() => onWaveUpdate.Call(_currentWaveIndex, wave, _currentPoints), CurrentInvasion.Name);
							}
						}
					}
				}
			}
        }

        private void OnReload(ReloadEventArgs args)
        {
            CurrentInvasion = null;
            lock(_lock)
            {
                foreach (var definition in _definitions)
                {
                    definition.Dispose();
                }
                _definitions.Clear();

                Utils.TryExecuteLua(LoadDefinitions, "InvasionManager");
            }
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
				lock( _lock )
				{
					var onWaveStart = CurrentInvasion.OnWaveStart;
					if( onWaveStart != null )
					{
						Utils.TryExecuteLua(() => onWaveStart.Call(_currentWaveIndex, wave), CurrentInvasion.Name);
					}
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
				lock( _lock )
				{
					var onWaveEnd = CurrentInvasion.OnWaveEnd;
					if( onWaveEnd != null )
					{
						Utils.TryExecuteLua(() => onWaveEnd.Call(previousWaveIndex, previousWave), CurrentInvasion.Name);
					}
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
