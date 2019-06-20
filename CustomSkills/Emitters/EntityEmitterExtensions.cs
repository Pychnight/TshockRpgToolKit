using CustomNpcs.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	public static class EntityEmitterExtensions
	{
		public static void Update(this IEnumerable<EntityEmitter> emitters)
		{
			foreach(var e in emitters)
				e.Update();
		}

		public static void Destroy(this IEnumerable<EntityEmitter> emitters)
		{
			foreach(var e in emitters)
				e.Destroy();
		}

		public static CustomProjectile SpawnCustomProjectile(this EntityEmitter emitter, string projectileName)
		{
			var emitPos = emitter.Position + emitter.EmitOffset;
			var unitDir = Vector2.Zero;
			
			if(emitter.UseRelativeTarget)
			{
				//must treat target as relative to our EmitPos..
				unitDir = emitter.Target; 
			}
			else
			{
				unitDir = emitter.Target - emitPos;
			}
			
			unitDir.Normalize();
			
			var emitVel = unitDir * emitter.EmitVelocity;

			return emitter.SpawnCustomProjectile(projectileName, emitPos.X, emitPos.Y, emitVel.X, emitVel.Y);
		}

		public static CustomProjectile SpawnCustomProjectile(this EntityEmitter emitter, string projectileName, float x, float y, float xSpeed, float ySpeed)
		{
			var owner = emitter.Parent?.whoAmI ?? 0;
			var result = CustomNpcs.ProjectileFunctions.SpawnCustomProjectile(owner, projectileName, x, y, xSpeed, ySpeed);

			return result;
		}

		//public static void RadialPosition(this EntityEmitter emitter, float angle, float xRadius, float yRadius)
		//{

		//}
	}
}
