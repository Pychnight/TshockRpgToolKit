using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
