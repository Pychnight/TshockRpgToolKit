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
        ///     Adds points for the specified custom NPC type.
        /// </summary>
        /// <param name="customNpcType">The custom NPC type, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="customNpcType" /> is <c>null</c>.</exception>
        public void AddPoints([NotNull] string customNpcType)
        {
            if (customNpcType == null)
            {
                throw new ArgumentNullException(nameof(customNpcType));
            }

            if (CurrentInvasion == null)
            {
                return;
            }
            CurrentInvasion.CustomNpcPointValues.TryGetValue(customNpcType, out var points);
            _currentPoints += points;

            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            var maxPoints = currentWave.PointsRequired;
            TSPlayer.All.SendData(PacketTypes.ReportInvasionProgress, "", Math.Min(_currentPoints, maxPoints),
                maxPoints, 0, _currentWaveIndex + 1);
        }

        /// <summary>
        ///     Adds points for the specified NPC type.
        /// </summary>
        /// <param name="npcType">The NPC type.</param>
        public void AddPoints(int npcType)
        {
            if (CurrentInvasion == null)
            {
                return;
            }
            CurrentInvasion.NpcPointValues.TryGetValue(npcType, out var points);
            _currentPoints += points;

            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            var maxPoints = currentWave.PointsRequired;
            TSPlayer.All.SendData(PacketTypes.ReportInvasionProgress, "", Math.Min(_currentPoints, maxPoints),
                maxPoints, 0, _currentWaveIndex + 1);
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
                    Utils.TryExecuteLua(definition.LoadLuaDefinition);
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
        ///     Starts the specified invasion.
        /// </summary>
        /// <param name="invasion">The invasion, or <c>null</c> to stop the current invasion.</param>
        public void StartInvasion([CanBeNull] InvasionDefinition invasion)
        {
            if (invasion != null)
            {
                TSPlayer.All.SendMessage(invasion.Waves[0].StartMessage, new Color(175, 75, 225));
                TSPlayer.All.SendData(PacketTypes.ReportInvasionProgress, "", 0, invasion.Waves[0].PointsRequired, 0,
                    1);
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
        /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
        public bool TrySpawnNpc([NotNull] TSPlayer player, int tileX, int tileY)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (CurrentInvasion == null)
            {
                return false;
            }

            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            if (player.TPlayer.activeNPCs >= currentWave.MaxSpawns || _random.Next(currentWave.SpawnRate) != 0)
            {
                return false;
            }

            var npcWeights = currentWave.NpcWeights;
            var totalWeight = npcWeights.Values.Sum();
            var customNpcWeights = currentWave.CustomNpcWeights;
            var totalCustomWeight = customNpcWeights.Values.Sum();
            var rand = _random.Next(totalWeight + totalCustomWeight);
            var current = 0;
            foreach (var kvp in npcWeights)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    var npcIndex = NPC.NewNPC(16 * tileX + 8, 16 * tileY, kvp.Key);
                    return npcIndex != Main.maxNPCs;
                }
                current += weight;
            }
            foreach (var kvp in customNpcWeights)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    var definition = NpcManager.Instance.FindDefinition(kvp.Key);
                    if (definition == null)
                    {
                        return false;
                    }

                    var customNpc = NpcManager.Instance.SpawnCustomNpc(definition, 16 * tileX + 8, tileY);
                    return customNpc != null;
                }
                current += weight;
            }
            return false;
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

            NPC.noSpawnCycle = true;
            var waves = CurrentInvasion.Waves;
            var currentWave = waves[_currentWaveIndex];
            if (_currentPoints >= currentWave.PointsRequired)
            {
                if (++_currentWaveIndex == waves.Count)
                {
                    TSPlayer.All.SendMessage(CurrentInvasion.CompletedMessage, new Color(175, 75, 225));
                    CurrentInvasion = null;
                }
                else
                {
                    TSPlayer.All.SendMessage(waves[_currentWaveIndex].StartMessage, new Color(175, 75, 225));
                }
                _currentPoints = 0;
            }

            Utils.TryExecuteLua(() => { CurrentInvasion?.OnUpdate?.Call(); });
        }
    }
}
