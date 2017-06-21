using System;
using System.Collections.Generic;
using System.Linq;
using CustomNpcs.Definitions;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

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
        ///     Gets or sets the HP.
        /// </summary>
        public int Hp
        {
            get => Npc.life;
            set => Npc.life = value;
        }

        /// <summary>
        ///     Gets the wrapped NPC.
        /// </summary>
        public NPC Npc { get; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        public Vector2 Position => Npc.position;

        /// <summary>
        ///     Gets or sets the target.
        /// </summary>
        [CanBeNull]
        public TSPlayer Target
        {
            get => Npc.target < 0 || Npc.target > Main.maxPlayers ? null : TShock.Players[Npc.target];
            set => Npc.target = value?.Index ?? -1;
        }

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
        ///     Determines whether the NPC has line of sight to the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if there is direct line of sight; otherwise, <c>false</c>.</returns>
        public bool HasLineOfSight(Vector2 position) => Collision.CanHitLine(Position, 1, 1, position, 1, 1);

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

        /// <summary>
        ///     Shoots a projectile at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="type">The type.</param>
        /// <param name="damage">The damage.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="knockback">The knockback.</param>
        public void ShootProjectileAt(Vector2 position, int type, int damage, float speed, float knockback)
        {
            var projectileId = Projectile.NewProjectile(Position,
                (position - Position) * speed / Vector2.Distance(Position, position), type, damage,
                knockback);
            TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
        }

        /// <summary>
        ///     Forces the NPC to target the nearest player.
        /// </summary>
        public void TargetNearestPlayer()
        {
            Target = TShock.Players
                .Where(p => p != null && p.Active)
                .OrderBy(p => Vector2.DistanceSquared(Position, p.TPlayer.position))
                .FirstOrDefault();
        }
    }
}
