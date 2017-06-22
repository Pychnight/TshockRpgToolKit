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

        private List<CustomNpcDefinition> _definitions = new List<CustomNpcDefinition>();

        private NpcManager()
        {
        }


        /// <summary>
        ///     Gets the NPC manager instance.
        /// </summary>
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
        ///     Attaches the specified definition to the NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="npc" /> or <paramref name="definition" /> is <c>null</c>.
        /// </exception>
        /// <returns>The custom NPC.</returns>
        [NotNull]
        public CustomNpc AttachCustomNpc([NotNull] NPC npc, [NotNull] CustomNpcDefinition definition)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            definition.ApplyTo(npc);
            if (_customNpcs.TryGetValue(npc, out _))
            {
                _customNpcs.Remove(npc);
            }

            var customNpc = new CustomNpc(npc, definition);
            _customNpcs.Add(npc, customNpc);
            return customNpc;
        }

        /// <summary>
        ///     Finds the definition with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The definition, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [CanBeNull]
        public CustomNpcDefinition FindDefinition([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Gets the custom NPC for the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <returns>The custom NPC, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        [CanBeNull]
        public CustomNpc GetCustomNpc([NotNull] NPC npc) =>
            _customNpcs.TryGetValue(npc, out var customNpc) ? customNpc : null;

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
                _definitions = JsonConvert.DeserializeObject<List<CustomNpcDefinition>>(File.ReadAllText(path));
                foreach (var definition in _definitions)
                {
                    definition.LoadLuaDefinition();
                }
            }
        }

        /// <summary>
        ///     Removes the custom NPC attached to the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public void RemoveCustomNpc([NotNull] NPC npc)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

            _customNpcs.Remove(npc);
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
        ///     Spawns a custom NPC at the specified coordinates.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition" /> is <c>null</c>.</exception>
        /// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
        [CanBeNull]
        public CustomNpc SpawnCustomNpc([NotNull] CustomNpcDefinition definition, int x, int y)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var npcId = NPC.NewNPC(x, y, definition.BaseType);
            if (npcId == Main.maxNPCs)
            {
                return null;
            }

            var npc = Main.npc[npcId];
            var customNpc = AttachCustomNpc(npc, definition);
            var onSpawn = customNpc.Definition.OnSpawn;
            Utils.TryExecuteLua(() => { onSpawn?.Call(customNpc); });
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
            TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);
            return customNpc;
        }

        /// <summary>
        ///     Tries to replace the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public bool TryReplaceNpc([NotNull] NPC npc)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

            var baseType = npc.netID;
            foreach (var definition in _definitions.Where(d => d.ReplacementTargetType == baseType))
            {
                if (_random.NextDouble() < (definition.ReplacementChance ?? 0.0))
                {
                    if (definition.BaseType != baseType)
                    {
                        npc.SetDefaults(definition.BaseType);
                    }
                    var customNpc = AttachCustomNpc(npc, definition);
                    var npcId = npc.whoAmI;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
                    TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);

                    var onSpawn = customNpc.Definition.OnSpawn;
                    Utils.TryExecuteLua(() => { onSpawn?.Call(customNpc); });
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Tries to spawn an NPC at the specified tile coordinates on the player.
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

            if (player.TPlayer.activeNPCs >= Config.Instance.MaxSpawns || _random.Next(Config.Instance.SpawnRate) != 0)
            {
                return false;
            }

            var weights = new Dictionary<CustomNpcDefinition, int>();
            foreach (var definition in _definitions.Where(d => d.ShouldCustomSpawn))
            {
                var canSpawn = false;
                var onCheckSpawn = definition.OnCheckSpawn;
                Utils.TryExecuteLua(() => { canSpawn = (bool)(onCheckSpawn?.Call(player, tileX, tileY)?[0] ?? true); });
                if (canSpawn)
                {
                    weights[definition] = definition.CustomSpawnWeight ?? 1;
                }
            }

            var rand = _random.Next(weights.Values.Sum());
            var current = 0;
            foreach (var kvp in weights)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    var customNpc = SpawnCustomNpc(kvp.Key, 16 * tileX + 8, 16 * tileY);
                    return customNpc != null;
                }
                current += weight;
            }
            return false;
        }
    }
}
