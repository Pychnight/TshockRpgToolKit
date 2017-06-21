using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CustomNpcs.Definitions;
using JetBrains.Annotations;
using Terraria;
using TShockAPI;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the custom NPC manager. This class is a singleton.
    /// </summary>
    [PublicAPI]
    public sealed class CustomNpcManager : IDisposable
    {
        private readonly ConditionalWeakTable<NPC, CustomNpc> _customNpcs = new ConditionalWeakTable<NPC, CustomNpc>();
        private readonly List<CustomNpcDefinition> _definitions = new List<CustomNpcDefinition>();

        private CustomNpcManager()
        {
        }

        /// <summary>
        ///     Gets the custom NPC manager.
        /// </summary>
        public static CustomNpcManager Instance { get; } = new CustomNpcManager();

        /// <summary>
        ///     Gets a read-only view of the definitions.
        /// </summary>
        [NotNull]
        public ReadOnlyCollection<CustomNpcDefinition> Definitions => _definitions.AsReadOnly();

        /// <summary>
        ///     Disposes the custom NPC manager.
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
        ///     Adds the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition" /> is <c>null</c>.</exception>
        public void AddDefinition([NotNull] CustomNpcDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            _definitions.Add(definition);
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
        ///     Spawns a custom mob at the specified coordinates.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition" /> is <c>null</c>.</exception>
        /// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
        [CanBeNull]
        public CustomNpc SpawnCustomMob([NotNull] CustomNpcDefinition definition, int x, int y)
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
    }
}
