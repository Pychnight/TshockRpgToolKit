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
    public sealed class InvasionManager : IDisposable
    {
        private readonly Random _random = new Random();

        private int _currentPoints;
        private int _currentWaveIndex;
        private List<InvasionDefinition> _definitions = new List<InvasionDefinition>();
        private bool _hasMiniboss;
        private DateTime _lastProgressUpdate;
        private int _requiredPoints;

        private InvasionManager()
        {
        }

        /// <summary>
        ///     Gets the invasion manager instance.
        /// </summary>
        [NotNull]
        public static InvasionManager Instance { get; } = new InvasionManager();

        /// <summary>
        ///     Gets the current invasion, or <c>null</c> if there is none.
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
            CurrentInvasion = invasion;
            if (CurrentInvasion != null)
            {
                _currentWaveIndex = 0;
                StartCurrentWave();
            }
        }

        internal void AddPoints(string npcType)
        {
            if (CurrentInvasion == null)
            {
                return;
            }
            CurrentInvasion.NpcPointValues.TryGetValue(npcType, out var points);

            if (_hasMiniboss && npcType.Equals(CurrentInvasion.Waves[_currentWaveIndex].Miniboss,
                    StringComparison.OrdinalIgnoreCase))
            {
                _hasMiniboss = false;
            }

            if (points > 0)
            {
                _currentPoints += points;
                _currentPoints = Math.Min(_currentPoints, _requiredPoints);
                NotifyRelevantPlayers();
            }
        }

        internal void LoadDefinitions(string path)
        {
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

        internal void SaveDefinitions(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(_definitions, Formatting.Indented));
        }

        internal bool ShouldSpawnInvasionNpcs(TSPlayer player)
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

        internal void TrySpawnInvasionNpc(TSPlayer player, int tileX, int tileY)
        {
            // ReSharper disable once PossibleNullReferenceException
            var currentWave = CurrentInvasion.Waves[_currentWaveIndex];
            if (player.TPlayer.activeNPCs >= currentWave.MaxSpawns || _random.Next(currentWave.SpawnRate) != 0)
            {
                return;
            }

            if (_currentPoints == _requiredPoints && _hasMiniboss)
            {
                var miniboss = currentWave.Miniboss;
                var isVanilla = int.TryParse(miniboss, out var npcType);
                foreach (var npc in Main.npc.Where(n => n?.active == true))
                {
                    if (isVanilla && npc.netID == npcType)
                    {
                        return;
                    }

                    var customNpc = NpcManager.Instance.GetCustomNpc(npc);
                    if (customNpc?.Definition.Name.Equals(miniboss, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return;
                    }
                }

                SpawnVanillaOrCustomNpc(miniboss, tileX, tileY);
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
                    SpawnVanillaOrCustomNpc(kvp.Key, tileX, tileY);
                    return;
                }
                current += weight;
            }
        }

        internal void UpdateInvasion()
        {
            if (CurrentInvasion == null)
            {
                return;
            }

            var waves = CurrentInvasion.Waves;
            if (_currentPoints == _requiredPoints && !_hasMiniboss)
            {
                if (++_currentWaveIndex == waves.Count)
                {
                    TSPlayer.All.SendMessage(CurrentInvasion.CompletedMessage, new Color(175, 75, 225));
                    CurrentInvasion = null;
                    return;
                }

                StartCurrentWave();
            }

            // Every second, send a ReportInvasionProgress packet to all players relevant to the invasion.
            var now = DateTime.UtcNow;
            if (now - _lastProgressUpdate > TimeSpan.FromSeconds(1))
            {
                NotifyRelevantPlayers();
                _lastProgressUpdate = now;
            }

            // ReSharper disable once PossibleNullReferenceException
            Utils.TryExecuteLua(() => CurrentInvasion.OnUpdate?.Call());
        }

        private void NotifyRelevantPlayers()
        {
            foreach (var player in TShock.Players.Where(p => p != null && p.Active && ShouldSpawnInvasionNpcs(p)))
            {
                player.SendData(PacketTypes.ReportInvasionProgress, "", _currentPoints, _requiredPoints, 0,
                    _currentWaveIndex + 1);
            }
        }

        private void SpawnVanillaOrCustomNpc(string npc, int tileX, int tileY)
        {
            if (int.TryParse(npc, out var npcType))
            {
                NPC.NewNPC(16 * tileX + 8, 16 * tileY, npcType);
                return;
            }

            var definition = NpcManager.Instance.FindDefinition(npc);
            if (definition != null)
            {
                NpcManager.Instance.SpawnCustomNpc(definition, 16 * tileX + 8, 16 * tileY);
            }
        }

        private void StartCurrentWave()
        {
            // ReSharper disable once PossibleNullReferenceException
            var wave = CurrentInvasion.Waves[_currentWaveIndex];
            TSPlayer.All.SendMessage(wave.StartMessage, new Color(175, 75, 225));
            _currentPoints = 0;
            _hasMiniboss = wave.Miniboss != null;
            _requiredPoints = wave.PointsRequired * (CurrentInvasion.ScaleByPlayers ? TShock.Utils.ActivePlayers() : 1);
        }
    }
}
