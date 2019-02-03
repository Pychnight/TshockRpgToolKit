using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNpcs.Projectiles;
using Terraria;

namespace CustomNpcs
{
	public static class ProjectileFunctions
	{
		/// <summary>
		///     Spawns a custom projectile with the specified parameters.
		/// </summary>
		/// <param name="name">The name, which must be a valid projectile name and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		/// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
		public static CustomProjectile SpawnCustomProjectile(int owner, string name, float x, float y, float xSpeed, float ySpeed)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			
			var definition = ProjectileManager.Instance?.FindDefinition(name);
			if (definition == null)
				throw new FormatException($"Invalid CustomProjectile name '{name}'.");
						
			return ProjectileManager.Instance.SpawnCustomProjectile(definition, x, y, xSpeed, ySpeed, owner);
		}

		/// <summary>
		///     Spawns a number of custom projectiles with the specified parameters.
		/// </summary>
		/// <param name="name">The name, which must be a valid projectile name and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		/// <returns>The custom projectiles, or <c>null</c> if spawning failed.</returns>
		public static Projectile[] SpawnCustomProjectile(int owner, string name, float x, float y, float xSpeed, float ySpeed, int amount)
		{
			var projectiles = new List<Projectile>(amount);

			for (var i = 0; i < amount; i++)
			{
				var cp = SpawnCustomProjectile(owner, name, x, y, xSpeed, ySpeed);

				if (cp != null)
					projectiles.Add(cp.Projectile);
			}

			return projectiles.ToArray();
		}
	}
}
