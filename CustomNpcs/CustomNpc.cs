using System;
using System.Collections.Generic;
using CustomNpcs.Definitions;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents a custom NPC.
    /// </summary>
    [PublicAPI]
    public sealed class CustomNpc
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomNpc" /> class with the specified NPC and definition.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <param name="definition">The custom NPC definition, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="npc" /> or <paramref name="definition" /> is <c>null</c>.
        /// </exception>
        public CustomNpc([NotNull] NPC npc, [NotNull] CustomNpcDefinition definition)
        {
            Npc = npc ?? throw new ArgumentNullException(nameof(npc));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        ///     Gets the custom NPC definition.
        /// </summary>
        public CustomNpcDefinition Definition { get; }

        /// <summary>
        ///     Gets the HP.
        /// </summary>
        public int Hp => Npc.life;

        /// <summary>
        ///     Gets the wrapped NPC.
        /// </summary>
        public NPC Npc { get; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        public Vector2 Position => Npc.position;

        /// <summary>
        ///     Gets the variable with the specified name.
        /// </summary>
        /// <param name="variableName">The name, which must not be <c>null</c>.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, or <paramref name="defaultValue" /> if the variable does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
        public object GetVariable([NotNull] string variableName, object defaultValue = null)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }

            return _variables.TryGetValue(variableName, out var value) ? value : defaultValue;
        }

        /// <summary>
        ///     Sets the variable with the specified name.
        /// </summary>
        /// <param name="variableName">The name, which must not be <c>null</c>.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
        public void SetVariable([NotNull] string variableName, object value)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }

            _variables[variableName] = value;
        }
    }
}
