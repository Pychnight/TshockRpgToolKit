using CustomNpcs.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace CustomSkills
{
	public class EntityEmitter
	{
		/// <summary>
		/// Gets or sets the parent Entity this Emitter belongs to.
		/// </summary>
		public Entity Parent { get; set; }

		/// <summary>
		/// World position of the Emitter.
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// Position of the Emitter, relative to Parent, if one exists and UseRelativePositioning is enabled.
		/// This allows for emitters to follow their parent's movement.
		/// </summary>
		public Vector2 RelativePosition;

		///// <summary>
		///// A vector representing the direction of the Emitter, that may be used when emitting entities.
		///// </summary>
		//public Vector2 Direction;

		/// <summary>
		/// A vector representing the intended target.
		/// </summary>
		public Vector2 Target;

		/// <summary>
		/// An optional offset vector from Position, which can be used during Emit.
		/// </summary>
		public Vector2 EmitOffset;

		/// <summary>
		/// An optional scalar for that can be multiplied against Direction, for a final velocity vector.
		/// </summary>
		public float EmitVelocity = 1f;

		/// <summary>
		/// Gets or sets whether this EntityEmitter's position is automatically moved relative to its Parent.
		/// </summary>
		public bool UseRelativePositioning { get; set; } = true;

		/// <summary>
		/// Gets or sets whether this EntityEmitter's target is relative or absolute;
		/// </summary>
		public bool UseRelativeTarget { get; set; } = false;

		/// <summary>
		/// Gets or sets if this emitter will continue function with no parent, or if parent is no longer active.
		/// </summary>
		public bool CanOutliveParent { get; set; }
		public bool IsActive { get; private set; }
		public TimeSpan Lifetime = TimeSpan.FromSeconds(5);
		public TimeSpan Cooldown = TimeSpan.FromSeconds(1);
		public DateTime LastEmitTime;
		public DateTime CreationTime;

		/// <summary>
		/// Gets or sets an optional string for identification. This is only for user convienience, and not use by any code.
		/// </summary>
		public string Name { get; set; } = "";
				
		public event Action<EntityEmitter> Emit;

		public EntityEmitter() : this(default(Entity))
		{
		}

		public EntityEmitter(TSPlayer player) : this(player.TPlayer)
		{
		}

		public EntityEmitter(Entity parent)
		{
			Parent = parent;
			LastEmitTime = DateTime.Now;
			CreationTime = DateTime.Now;
			IsActive = true;
		}

		public void Destroy()
		{
			IsActive = false;
		}

		public void Update()
		{
			if(!IsActive)
				return;

			var now = DateTime.Now;

			if(!CanOutliveParent)
			{
				if(Parent == null || Parent.active == false)
				{
					Destroy();
					return;
				}
			}

			if((now - CreationTime) >= Lifetime)
			{
				Destroy();
				return;
			}

			if(UseRelativePositioning)
			{
				if(Parent.active)
				{
					Position.X = Parent.position.X + RelativePosition.X;
					Position.Y = Parent.position.Y + RelativePosition.Y;
				}
			}

			//are we ready to emit?
			if((now - LastEmitTime) >= Cooldown)
			{
				//do emit
				//Debug.Print("Emit Entity!");
				Emit?.Invoke(this);

				//start cooldown...
				LastEmitTime = now;
			}
		}

		public void SetTarget(float x, float y, bool relativeTarget)
		{
			Target.X = x;
			Target.Y = y;
			UseRelativeTarget = relativeTarget;
		}

		public void SetAngle(float angleDegrees)
		{
			var radians  = Math.PI * angleDegrees / 180.0d;
			var x = (float)Math.Cos(radians);
			var y = (float)Math.Sin(radians);

			SetTarget(x, y, true);
		}

		public void FaceUp()			=> SetTarget(0, -1, true);
		public void FaceDown()			=> SetTarget(0, 1, true);
		public void FaceRight()			=> SetTarget(1, 0, true);
		public void FaceLeft()			=> SetTarget(-1, 0, true);
		public void FaceTopLeft()		=> SetTarget(-1, -1, true);
		public void FaceTopRight()		=> SetTarget(1, -1, true);
		public void FaceBottomRight()	=> SetTarget(1, 1, true);
		public void FaceBottomLeft()	=> SetTarget(-1, 1, true);
	}
}
