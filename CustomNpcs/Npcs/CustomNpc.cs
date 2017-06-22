using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Npcs
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
            get => Npc.target < 0 || Npc.target >= Main.maxPlayers ? null : TShock.Players[Npc.target];
            set => Npc.target = value?.Index ?? -1;
        }

        /// <summary>
        ///     Buffs the nearby players within the specified radius.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="seconds">The seconds, which must be positive.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="seconds" /> or <paramref name="radius" /> is not positive.
        /// </exception>
        public void BuffNearbyPlayers(int type, int seconds, int radius)
        {
            if (seconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
            }
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
            }

            foreach (var player in TShock.Players.Where(
                p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
                     256 * radius * radius))
            {
                player.SetBuff(type, 60 * seconds, true);
            }
        }

        /// <summary>
        ///     Transforms the NPC to the specified custom NPC.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        public void CustomTransform([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var definition = NpcManager.Instance.FindDefinition(name);
            if (definition == null)
            {
                throw new FormatException($"Invalid custom NPC name '{name}'.");
            }

            Npc.SetDefaults(definition.BaseType);
            var customNpc = NpcManager.Instance.AttachCustomNpc(Npc, definition);
            var npcId = Npc.whoAmI;
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
            TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);

            var onSpawn = customNpc.Definition.OnSpawn;
            Utils.TryExecuteLua(() => { onSpawn?.Call(customNpc); });
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
        ///     Determines whether the variable with the specified name exists.
        /// </summary>
        /// <param name="variableName">The name, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
        public bool HasVariable([NotNull] string variableName)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }

            return _variables.ContainsKey(variableName);
        }

        /// <summary>
        ///     Messages the nearby players within the specified radius.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="radius" /> is not positive.</exception>
        public void MessageNearbyPlayers([NotNull] string message, Color color, int radius)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
            }

            foreach (var player in TShock.Players.Where(
                p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
                     256 * radius * radius))
            {
                player.SendMessage(message, color);
            }
        }

        /// <summary>
        ///     Ensures that the specified callback is run only once.
        /// </summary>
        /// <param name="key">The key, which must be unique.</param>
        /// <param name="callback">The callback, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback" /> is <c>null</c>.</exception>
        public void OnlyOnce(int key, [NotNull] LuaFunction callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!HasVariable($"OnlyOnce{key}"))
            {
                SetVariable($"OnlyOnce{key}", true);
                callback.Call();
            }
        }

        /// <summary>
        ///     Runs the specified callback periodically.
        /// </summary>
        /// <param name="key">The key, which must be unique.</param>
        /// <param name="callback">The callback, which must not be <c>null</c>.</param>
        /// <param name="period">The period, which must be positive.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period" /> is not positive.</exception>
        public void Periodically(int key, [NotNull] LuaFunction callback, int period)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (period <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), "Period must be positive.");
            }

            var timer = (int)GetVariable($"Periodically{key}", 0);
            if (timer++ == 0)
            {
                callback.Call();
            }

            timer %= period;
            SetVariable($"Periodically{key}", timer);
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
                (position - Position) * speed / Vector2.Distance(Position, position), type, damage, knockback);
            TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
        }

        /// <summary>
        ///     Forces the NPC to target the closest player.
        /// </summary>
        public void TargetClosestPlayer()
        {
            Npc.TargetClosest();
        }

        /// <summary>
        ///     Teleports the NPC to the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        public void Teleport(Vector2 position)
        {
            Npc.Teleport(position);
        }
    }
}
