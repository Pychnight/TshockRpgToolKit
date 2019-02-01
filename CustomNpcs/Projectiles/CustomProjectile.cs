using Microsoft.Xna.Framework;
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
	public class CustomProjectile : CustomEntity<Projectile,ProjectileDefinition>
	{
		public Projectile Projectile => Entity;
				
		/// <summary>
		///     Gets the index.
		/// </summary>
		public int Index => Projectile.identity;//we apparently use identity over whoAmI with projectiles(??) ...terraria.

		public bool Active
		{
			get => Projectile.active;
		}
				
		/// <summary>
		///     Gets or sets a value indicating whether to send a network update.
		/// </summary>
		public bool SendNetUpdate
		{
			get => Projectile.netUpdate;
			set => Projectile.netUpdate = value;
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
			Entity = projectile;
			Definition = definition;
		}
		
		public void Kill()
		{
			Projectile.Kill();
			//Debug.Print($"Manually killed projectile #{Index}");
		}

		public void BasicUpdate()
		{
			//TimeLeft--;
			OldDirection = Direction;
			OldPosition = Position;
			Position = Position + Velocity;
		}

		public bool CustomIDContains(string text)
		{
			return this.Definition?.Name.Contains(text)==true;
		}
	}
}
