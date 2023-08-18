using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace CustomNpcs
{
	/// <summary>
	/// Common base class for custom Terraria.Entity wrappers.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TCustomDefinition"></typeparam>
	public abstract class CustomEntity<TEntity, TCustomDefinition>
												where TEntity : Entity
												where TCustomDefinition : DefinitionBase
	{
		protected internal TEntity Entity { get; set; }
		private Dictionary<string, object> _variables = new Dictionary<string, object>();

		/// <summary>
		///     Gets the custom entity definition.
		/// </summary>
		public TCustomDefinition Definition { get; set; }

		/// <summary>
		///     Gets the Center of the Entity and sets it.
		///     Useful for Custom AI
		/// </summary>
		public Vector2 Center
		{
			get => Entity.Center;
			set => Entity.Center = value;
		}

		/// <summary>
		///     Gets or sets the position.
		/// </summary>
		public Vector2 Position
		{
			get => Entity.position;
			set => Entity.position = value;
		}

		/// <summary>
		///     Gets or sets the old position.
		/// </summary>
		public Vector2 OldPosition
		{
			get => Entity.oldPosition;
			set => Entity.oldPosition = value;
		}

		/// <summary>
		///     Gets or sets the direction.
		/// </summary>
		public int Direction
		{
			get => Entity.direction;
			set => Entity.direction = value;
		}

		/// <summary>
		///     Gets or sets the old direction.
		/// </summary>
		public int OldDirection
		{
			get => Entity.oldDirection;
			set => Entity.oldDirection = value;
		}

		/// <summary>
		///     Gets or sets the velocity.
		/// </summary>
		public Vector2 Velocity
		{
			get => Entity.velocity;
			set => Entity.velocity = value;
		}

		/// <summary>
		/// Provides easy access to a CustomEntity's embedded variables.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				_variables.TryGetValue(key, out var result);
				return result;
			}
			set => _variables[key] = value;
		}

		public float AngleFrom(Vector2 source) => Entity.AngleFrom(source);
		public float AngleTo(Vector2 destination) => Entity.AngleTo(destination);
		public Vector2 DirectionFrom(Vector2 source) => Entity.DirectionFrom(source);
		public Vector2 DirectionTo(Vector2 destination) => Entity.DirectionTo(destination);
		public float Distance(Vector2 other) => Entity.Distance(other);
		public float DistanceSQ(Vector2 other) => Entity.DistanceSQ(other);
		public bool WithinRange(Vector2 target, float maxRange) => Entity.WithinRange(target, maxRange);

		/// <summary>
		///     Determines whether the variable with the specified name exists.
		/// </summary>
		/// <param name="variableName">The name, which must not be <c>null</c>.</param>
		/// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
		public bool HasVariable(string variableName)
		{
			if (variableName == null)
				throw new ArgumentNullException(nameof(variableName));

			return _variables.ContainsKey(variableName);
		}

		/// <summary>
		///     Gets the variable with the specified name.
		/// </summary>
		/// <param name="variableName">The name, which must not be <c>null</c>.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The value, or <paramref name="defaultValue" /> if the variable does not exist.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
		public object GetVariable(string variableName, object defaultValue = null)
		{
			if (variableName == null)
				throw new ArgumentNullException(nameof(variableName));

			return _variables.TryGetValue(variableName, out var value) ? value : defaultValue;
		}

		/// <summary>
		///     Sets the variable with the specified name.
		/// </summary>
		/// <param name="variableName">The name, which must not be <c>null</c>.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
		public void SetVariable(string variableName, object value)
		{
			if (variableName == null)
				throw new ArgumentNullException(nameof(variableName));

			_variables[variableName] = value;
		}

		/// <summary>
		///     Applies a callback to each player within the specified tile radius.
		/// </summary>
		/// <param name="callback">The callback, which must not be <c>null</c>.</param>
		/// <param name="tileRadius">The tile radius, which must be positive.</param>
		/// <exception cref="ArgumentNullException"><paramref name="callback" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="tileRadius" /> is not positive.</exception>
		public void ForEachNearbyPlayer(Action<TSPlayer> callback, int tileRadius = 50)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			if (tileRadius <= 0)
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");

			foreach (var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius))
			{
				callback?.Invoke(player);
			}
		}

		/// <summary>
		///     Buffs the nearby players within the specified tile radius.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="seconds">The seconds, which must be positive.</param>
		/// <param name="tileRadius">The tile radius, which must be positive.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="seconds" /> or <paramref name="tileRadius" /> is not positive.
		/// </exception>
		public void BuffNearbyPlayers(int type, int seconds, int tileRadius = 50)
		{
			if (seconds <= 0)
				throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");

			if (tileRadius <= 0)
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");

			foreach (var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius))
			{
				player.SetBuff(type, 60 * seconds, true);
			}
		}

		/// <summary>
		///     Messages the nearby players within the specified tile radius.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="color">The color.</param>
		/// <param name="tileRadius">The tile radius, which must be positive.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="tileRadius" /> is not positive.</exception>
		public void MessageNearbyPlayers(string message, Color color, int tileRadius = 50)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (tileRadius <= 0)
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");

			foreach (var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius))
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
		public void OnlyOnce(int key, Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			if (!HasVariable($"OnlyOnce{key}"))
			{
				SetVariable($"OnlyOnce{key}", true);
				callback?.Invoke();
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
		public void Periodically(int key, Action callback, int period)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			if (period <= 0)
				throw new ArgumentOutOfRangeException(nameof(period), "Period must be positive.");

			// ReSharper disable once PossibleNullReferenceException
			var timer = (int)GetVariable($"Periodically{key}", 0);
			if (timer++ == 0)
			{
				callback?.Invoke();
			}

			timer %= period;
			SetVariable($"Periodically{key}", timer);
		}
	}
}
