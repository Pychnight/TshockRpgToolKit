﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Projectiles
{
	public class CustomProjectile
	{
		private Dictionary<string, object> _variables = new Dictionary<string, object>();
		public Projectile Projectile { get; private set; }
		public ProjectileDefinition Definition { get; set; }

		/// <summary>
		/// Provides easy access to a CustomProjectile's embedded variables.
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
			set
			{
				_variables[key] = value;
			}
		}

		/// <summary>
		///     Gets the index.
		/// </summary>
		public int Index => Projectile.whoAmI;

		public bool Active
		{
			get => Projectile.active;
		}

		/// <summary>
		///     Gets the Center of the npc and sets it.
		///     Useful for Custom AI
		/// </summary>
		public Vector2 Center
		{
			get => Projectile.Center;
			set => Projectile.Center = value;
		}

		/// <summary>
		///     Gets or sets the old position.
		/// </summary>
		public Vector2 OldPosition
		{
			get => Projectile.oldPosition;
			set => Projectile.oldPosition = value;
		}

		/// <summary>
		///     Gets or sets the position.
		/// </summary>
		public Vector2 Position
		{
			get => Projectile.position;
			set => Projectile.position = value;
		}

		/// <summary>
		///     Gets or sets the old direction.
		/// </summary>
		public int OldDirection
		{
			get => Projectile.oldDirection;
			set => Projectile.oldDirection = value;
		}

		/// <summary>
		///     Gets or sets the direction.
		/// </summary>
		public int Direction
		{
			get => Projectile.direction;
			set => Projectile.direction = value;
		}
		
		/// <summary>
		///     Gets or sets a value indicating whether to send a network update.
		/// </summary>
		public bool SendNetUpdate
		{
			get => Projectile.netUpdate;
			set => Projectile.netUpdate = value;
		}

		/// <summary>
		///     Gets or sets the velocity.
		/// </summary>
		public Vector2 Velocity
		{
			get => Projectile.velocity;
			set => Projectile.velocity = value;
		}

		public int TimeLeft 
		{
			get => Projectile.timeLeft;
			set => Projectile.timeLeft = value;
		}

		public int Damage
		{
			get => Projectile.damage;
			set => Projectile.damage = value;
		}

		public float Knockback
		{
			get => Projectile.knockBack;
			set => Projectile.knockBack = value;
		}

		public int Owner
		{
			get => Projectile.owner;
			set => Projectile.owner = value;
		}

		public float[] Ai
		{
			get => Projectile.ai;
		}

		public CustomProjectile(Projectile projectile, ProjectileDefinition definition)
		{
			Projectile = projectile;
			Definition = definition;
		}

		/// <summary>
		///     Determines whether the variable with the specified name exists.
		/// </summary>
		/// <param name="variableName">The name, which must not be <c>null</c>.</param>
		/// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="variableName" /> is <c>null</c>.</exception>
		public bool HasVariable(string variableName)
		{
			if (variableName == null)
			{
				throw new ArgumentNullException(nameof(variableName));
			}

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
		public void SetVariable(string variableName, object value)
		{
			if (variableName == null)
			{
				throw new ArgumentNullException(nameof(variableName));
			}

			_variables[variableName] = value;
		}

		public void Kill()
		{
			Projectile.Kill();
		}

		public void BasicUpdate()
		{
			TimeLeft--;
			OldDirection = Direction;
			OldPosition = Position;
			Position = Position + Velocity;

			//ProjectileManager.SendProjectileUpdate(this.Index);
		}

		public void AttachEmote(int emoteId, int lifeTime)
		{
			EmoteFunctions.AttachEmote(EmoteFunctions.AnchorTypeProjectile, this.Index, emoteId, lifeTime);
		}

		public bool CustomIDContains(string text)
		{
			return this.Definition?.Name.Contains(text)==true;
		}
	}
}
