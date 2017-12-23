using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs.Projectiles
{
	public static class ProjectileFunctions
	{
		/// <summary>
		///     Spawns a custom projectile with the specified name at a position.
		/// </summary>
		/// <param name="name">The name, which must be a valid projectile name and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		/// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
		[LuaGlobal]
		public static CustomProjectile SpawnCustomProjectile(int owner, string name, Vector2 position, Vector2 speed)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			var definition = ProjectileManager.Instance?.FindDefinition(name);
			if (definition == null)
			{
				throw new FormatException($"Invalid custom projectile name '{name}'.");
			}
			
			return ProjectileManager.Instance.SpawnCustomProjectile(definition, position.X, position.Y, speed.X, speed.Y, owner);
		}
	}
}
