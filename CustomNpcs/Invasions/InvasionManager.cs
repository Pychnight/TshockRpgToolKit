using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomNpcs.Npcs;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents an invasion manager. This class is a singleton.
    /// </summary>
    [PublicAPI]
    public class InvasionManager : IDisposable
    {
        private readonly Random _random = new Random();

        private int _currentPoints;
        private int _currentWaveIndex;
        private List<InvasionDefinition> _definitions = new List<InvasionDefinition>();

        private DateTime _lastProgressUpdate;
        private int _requiredPoints;

        private InvasionManager()
        {
        }

        /// <summary>
        ///     Gets the invasion manager instance.
        /// </summary>
        public static InvasionManager Instance { get; } = new InvasionManager();

        /// <summary>
        ///     Gets the current invasion.
        /// </summary>
        [CanBeNull]
        public InvasionDefinition CurrentInvasion { get; private set; }

        /// <summary>
        ///     Disposes the invasion manager.
        /// </summary>
        public void Dispose()
        {
            CurrentInvasion = null;
            foreach (var definition in _definitions)
            {
                definition.Dispose();
            }
            _definitions.Clear();
        }

        /// <summary>
        ///     Adds points for the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public void AddPoints([NotNull] string npc)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

            if (CurrentInvasion == null)
            {
                return;
            }
            CurrentInvasion.NpcPointValues.TryGetValue(npc, out var points);
            _currentPoints += points;

            if (points > 0)
            {
                TSPlayer.All.SendData(PacketTypes.ReportInvasionProgress, "", Math.Min(_currentPoints, _requiredPoints),
                    _requiredPoints, 0, _currentWaveIndex + 1);
            }
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
        ///     Loads the definitions from the specified path.
        /// </summary>
        /// <param name="path">The path, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c>.</exception>
        /// <exception cref="FormatException">There is a malformed definition.</exception>
        public void LoadDefinitions([NotNull] string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (File.Exists(path))
            {
                _definitions = JsonConvert.DeserializeObject<List<InvasionDefinition>>(File.ReadAllText(path));
                foreach (var definition in _definitions)
                {
                    definition.ThrowIfInvalid();
                    definition.LoadLuaDefinition();
                }
            }
        }

        /// <summary>
        ///     Saves the definitions to the specified path.
        /// </summary>
        /// <param name="path">The path, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c>.</exception>
        public void SaveDefinitions([NotNull] string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(_definitions, Formatting.Indented));
        }

        /// <summary>
        ///     Determines whether an invasion NPC should spawn for the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if an invasion NPC should spawn; otherwise, <c>false</c>.</returns>
        public bool ShouldSpawn([NotNull] TSPlayer player)
        {
            if (CurrentInvasion == null)
            {
                return false;
            }

            var playerPosition = player.TPlayer.position;
            return !CurrentInvasion.AtSpawnOnly || Main.spawnTileX * 16.0 - 3000 < playerPosition.X &&
                   playerPosition.X < Main.spawnTileX * 16.0 + 3000 &&
                   playerPosition.Y < Main.worldSurface * 16.0 + NPC.sHeight;
        }

        /// <summary>
        ///     Starts the specified invasion.
        /// </summary>
        /// <param name="invasion">The invasion, or <c>null</c> to stop the current invasion.</param>
        public void StartInvasion([CanBeNull] InvasionDefinition invasion)
        {
            if (invasion != null)
            {
                TSPlayer.All.SendMessage(invasion.Waves[0].StartMessage, new Color(175, 75, 225));
                _requiredPoints = invasion.Waves[0].PointsRequired;
                if (invasion.ScaleByPlayers)
                {
                    _requiredPoints *= TShock.Utils.ActivePlayers();
                }
            }
            _currentWaveIndex = 0;
            _currentPoints = 0;
            CurrentInvasion = invasion;
        }

        /// <summary>
        ///     Tries to spawn an invasion NPC at the specified tile coordinates on the player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="tileX">The X coordinate.</param>
        /// <param name="tileY">The Y coordinate.</param>
        public void TrySpawnNpc([NotNull] TSPlayer player, int tileX, int tileY)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (CurrentInvasion == null)
            {
                return;
            }

            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            if (player.TPlayer.activeNPCs >= currentWave.MaxSpawns || _random.Next(currentWave.SpawnRate) != 0)
            {
                return;
            }

            var npcWeights = currentWave.NpcWeights;
            var rand = _random.Next(npcWeights.Values.Sum());
            var current = 0;
            foreach (var kvp in npcWeights)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    var npc = kvp.Key;
                    if (int.TryParse(npc, out var npcType))
                    {
                        NPC.NewNPC(16 * tileX + 8, 16 * tileY, npcType);
                        return;
                    }

                    var definition = NpcManager.Instance.FindDefinition(npc);
                    if (definition == null)
                    {
                        return;
                    }

                    NpcManager.Instance.SpawnCustomNpc(definition, 16 * tileX + 8, tileY);
                    return;
                }
                current += weight;
            }
        }

        /// <summary>
        ///     Updates the invasion.
        /// </summary>
        public void UpdateInvasion()
        {
            if (CurrentInvasion == null)
            {
                return;
            }

            var waves = CurrentInvasion.Waves;
            if (_currentPoints >= _requiredPoints)
            {
                if (++_currentWaveIndex == waves.Count)
                {
                    TSPlayer.All.SendMessage(CurrentInvasion.CompletedMessage, new Color(175, 75, 225));
                    CurrentInvasion = null;
                }
                else
                {
                    TSPlayer.All.SendMessage(waves[_currentWaveIndex].StartMessage, new Color(175, 75, 225));
                    _requiredPoints = waves[_currentWaveIndex].PointsRequired;
                    if (CurrentInvasion.ScaleByPlayers)
                    {
                        _requiredPoints *= TShock.Utils.ActivePlayers();
                    }
                }
                _currentPoints = 0;
            }

            var now = DateTime.UtcNow;
            if (now - _lastProgressUpdate > TimeSpan.FromSeconds(1))
            {
                foreach (var player in TShock.Players.Where(p => p != null && p.Active))
                {
                    if (ShouldSpawn(player))
                    {
                        player.SendData(PacketTypes.ReportInvasionProgress, "", _currentPoints, _requiredPoints, 0,
                            _currentWaveIndex + 1);
                    }
                }
                _lastProgressUpdate = now;
            }

            Utils.TryExecuteLua(() => { CurrentInvasion?.OnUpdate?.Call(); });
        }
    }
}
