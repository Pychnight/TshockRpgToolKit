using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TShockAPI;

namespace CustomNpcs.Projectiles
{
	public delegate void ProjectileSpawnHandler(CustomProjectile projectile);
	public delegate void ProjectileKilledHandler(CustomProjectile projectile);
	public delegate bool ProjectileGameUpdateHandler(CustomProjectile projectile);
	public delegate bool ProjectileAiUpdateHandler(CustomProjectile projectile);
	public delegate void ProjectileCollisionHandler(CustomProjectile projectile, TSPlayer player);
	public delegate void ProjectileTileCollisionHandler(CustomProjectile projectile, List<Point> tileHits);
}
