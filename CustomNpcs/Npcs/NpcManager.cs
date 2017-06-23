using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents an NPC manager. This class is a singleton.
    /// </summary>
    [PublicAPI]
    public sealed class NpcManager : IDisposable
    {
        private readonly ConditionalWeakTable<NPC, CustomNpc> _customNpcs = new ConditionalWeakTable<NPC, CustomNpc>();
        private readonly Random _random = new Random();

        private List<NpcDefinition> _definitions = new List<NpcDefinition>();

        private NpcManager()
        {
        }

        /// <summary>
        ///     Gets the NPC manager instance.
        /// </summary>
        [NotNull]
        public static NpcManager Instance { get; } = new NpcManager();

        /// <summary>
        ///     Disposes the NPC manager.
        /// </summary>
        public void Dispose()
        {
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
        public NpcDefinition FindDefinition([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Spawns a custom NPC at the specified coordinates.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition" /> is <c>null</c>.</exception>
        /// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
        public CustomNpc SpawnCustomNpc([NotNull] NpcDefinition definition, int x, int y)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var npcId = NPC.NewNPC(x, y, definition.BaseType);
            return npcId != Main.maxNPCs ? AttachCustomNpc(Main.npc[npcId], definition) : null;
        }

        internal CustomNpc AttachCustomNpc(NPC npc, NpcDefinition definition)
        {
            if (definition.BaseType != npc.netID)
            {
                npc.SetDefaults(definition.BaseType);
            }
            definition.ApplyTo(npc);

            var customNpc = new CustomNpc(npc, definition);
            _customNpcs.Remove(npc);
            _customNpcs.Add(npc, customNpc);
            Utils.TryExecuteLua(() => definition.OnSpawn?.Call(customNpc));

            // Ensure that all players see the changes.
            var npcId = npc.whoAmI;
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
            TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);
            return customNpc;
        }

        internal CustomNpc GetCustomNpc(NPC npc) =>
            _customNpcs.TryGetValue(npc, out var customNpc) ? customNpc : null;

        internal void LoadDefinitions([NotNull] string path)
        {
            if (File.Exists(path))
            {
                _definitions = JsonConvert.DeserializeObject<List<NpcDefinition>>(File.ReadAllText(path));
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

        internal void TryReplaceNpc(NPC npc)
        {
            // Randomly pick a valid replacement definition, if possible.
            var definition = _definitions
                .Where(d => d.ReplacementTargetType == npc.netID)
                .FirstOrDefault(d => _random.NextDouble() < (d.ReplacementChance ?? 1));
            if (definition != null)
            {
                AttachCustomNpc(npc, definition);
            }
        }

        internal void TrySpawnCustomNpc(TSPlayer player, int tileX, int tileY)
        {
            if (player.TPlayer.activeNPCs >= Config.Instance.MaxSpawns || _random.Next(Config.Instance.SpawnRate) != 0)
            {
                return;
            }

            // Get spawn weights for all NPC definitions.
            var weights = new Dictionary<NpcDefinition, int>();
            foreach (var definition in _definitions.Where(d => d.ShouldCustomSpawn))
            {
                var weight = 0;
                Utils.TryExecuteLua(() => weight =
                    (int)((double?)definition.OnCheckSpawn?.Call(player, tileX, tileY)[0] ?? 0));
                weights[definition] = weight;
            }

            // Randomly pick an NPC to spawn.
            var rand = _random.Next(weights.Values.Sum());
            var current = 0;
            foreach (var kvp in weights)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    SpawnCustomNpc(kvp.Key, 16 * tileX + 8, 16 * tileY);
                    return;
                }
                current += weight;
            }
        }
    }
}
