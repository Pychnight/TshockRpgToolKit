using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using CustomNpcs.Projectiles;
using System.Diagnostics;
using Terraria.GameContent.UI;
using Terraria.Localization;
using Corruption;

namespace CustomNpcs.Npcs
{
	/// <summary>
	///     Represents a custom NPC.
	/// </summary>
	public sealed class CustomNpc
	{
		private Dictionary<string, object> _variables = new Dictionary<string, object>();

		internal bool IsNpcValid; // if false, the NPC wrapped by this CustomNpc is no longer valid for usage.

		//OTAPI runs its post transform hook before names have changed. This flag is a work around so we can poll elsewhere, 
		// and see if we want to do any truly post transform ops. Just make sure to reset it to false once any post ops have been performed.
		internal bool HasTransformed;

		/// <summary>
		/// Provides easy access to a CustomNpc's embedded variables.
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
		///     Initializes a new instance of the <see cref="CustomNpc" /> class with the specified NPC and definition.
		/// </summary>
		/// <param name="npc">The NPC, which must not be <c>null</c>.</param>
		/// <param name="definition">The custom NPC definition, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="npc" /> or <paramref name="definition" /> is <c>null</c>.
		/// </exception>
		public CustomNpc(NPC npc, NpcDefinition definition)
		{
			Npc = npc ?? throw new ArgumentNullException(nameof(npc));
			Definition = definition ?? throw new ArgumentNullException(nameof(definition));
			IsNpcValid = true;
		}

		/// <summary>
		///     Gets the custom NPC definition.
		/// </summary>
		public NpcDefinition Definition { get; }

		/// <summary>
		///     Gets the index.
		/// </summary>
		public int Index => Npc.whoAmI;

		/// <summary>
		///     Gets the wrapped NPC.
		/// </summary>
		public NPC Npc { get; }

		/// <summary>
		///     Gets or sets the HP.
		/// </summary>
		public int Hp
		{
			get => Npc.life;
			set => Npc.life = value;
		}

		/// <summary>
		///		Gets the LifeMax.
		/// </summary>
		public int MaxHp => Npc.lifeMax; 
		
		/// <summary>
		///     Gets the Center of the npc and sets it.
		///     Useful for Custom AI
		/// </summary>
		public Vector2 Center
		{
			get => Npc.Center;
			set => Npc.Center = value;
		}

		/// <summary>
		///     Gets or sets the old position.
		/// </summary>
		public Vector2 OldPosition
		{
			get => Npc.oldPosition;
			set => Npc.oldPosition = value;
		}

		/// <summary>
		///     Gets or sets the position.
		/// </summary>
		public Vector2 Position
		{
			get => Npc.position;
			set => Npc.position = value;
		}

		/// <summary>
		///     Gets or sets the old direction.
		/// </summary>
		public int OldDirection
		{
			get => Npc.oldDirection;
			set => Npc.oldDirection = value;
		}

		/// <summary>
		///     Gets or sets the old direction Y.
		/// </summary>
		public int OldDirectionY
		{
			get => Npc.oldDirectionY;
			set => Npc.oldDirectionY = value;
		}

		/// <summary>
		///     Gets or sets the direction.
		/// </summary>
		public int Direction
		{
			get => Npc.direction;
			set => Npc.direction = value;
		}

		/// <summary>
		///     Gets or sets the direction Y.
		/// </summary>
		public int DirectionY
		{
			get => Npc.directionY;
			set => Npc.directionY = value;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to send a network update.
		/// </summary>
		public bool SendNetUpdate
		{
			get => Npc.netUpdate;
			set => Npc.netUpdate = value;
		}

		/// <summary>
		///     Gets or sets the target.
		/// </summary>
		public TSPlayer Target
		{
			get => Npc.target < 0 || Npc.target >= Main.maxPlayers ? null : TShock.Players[Npc.target];
			set => Npc.target = value?.Index ?? -1;
		}

		/// <summary>
		///     Gets or sets the velocity.
		/// </summary>
		public Vector2 Velocity
		{
			get => Npc.velocity;
			set => Npc.velocity = value;
		}

		public string GivenName
		{
			get => Npc._givenName;
			set
			{
				Npc._givenName = value;
				TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", Npc.whoAmI);
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
			if( seconds <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
			}
			if( tileRadius <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");
			}

			foreach( var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius) )
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
		public void CustomTransform(string name)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}

			var definition = NpcManager.Instance?.FindDefinition(name);
			if( definition == null )
			{
				throw new FormatException($"Invalid custom NPC name '{name}'.");
			}

			NpcManager.Instance.AttachCustomNpc(Npc, definition);
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
			if( callback == null )
			{
				throw new ArgumentNullException(nameof(callback));
			}
			if( tileRadius <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");
			}

			foreach( var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius) )
			{
				callback?.Invoke(player);
			}
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
			if( variableName == null )
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
		public bool HasVariable(string variableName)
		{
			if( variableName == null )
			{
				throw new ArgumentNullException(nameof(variableName));
			}

			return _variables.ContainsKey(variableName);
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
			if( message == null )
			{
				throw new ArgumentNullException(nameof(message));
			}
			if( tileRadius <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(tileRadius), "Tile radius must be positive.");
			}

			foreach( var player in TShock.Players.Where(
				p => p != null && p.Active && Vector2.DistanceSquared(Position, p.TPlayer.position) <
					 256 * tileRadius * tileRadius) )
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
			if( callback == null )
			{
				throw new ArgumentNullException(nameof(callback));
			}

			if( !HasVariable($"OnlyOnce{key}") )
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
			if( callback == null )
			{
				throw new ArgumentNullException(nameof(callback));
			}
			if( period <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(period), "Period must be positive.");
			}

			// ReSharper disable once PossibleNullReferenceException
			var timer = (int)GetVariable($"Periodically{key}", 0);
			if( timer++ == 0 )
			{
				callback?.Invoke();
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
		public void SetVariable(string variableName, object value)
		{
			if( variableName == null )
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
				( position - Position ) * speed / Vector2.Distance(Position, position), type, damage, knockback);
			TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
		}

		public void ShootCustomProjectileAt(Vector2 target, string projectileName, float speed)
		{
			ShootCustomProjectileAt(target, projectileName, speed, new Vector2(0, 0));
		}

		public void ShootCustomProjectileAt(Vector2 target, string projectileName, float speed, Offset offset)
		{
			var v = offset.ToUnitVector();
			var hWidth = Npc.width * 0.5f;
			var hHeight = Npc.height * 0.5f;
			var offsetVector = new Vector2(v.X * hWidth, v.Y * hHeight);

			ShootCustomProjectileAt(target, projectileName, speed, offsetVector);
		}

		public void ShootCustomProjectileAt(Vector2 target, string projectileName, float speed, Vector2 offset)
		{
			int owner = this.Index;//how does owner affect projectiles? Not seeing difference when I change it to the launching npc.
			var start = Center + offset;
			var delta = target - start;

			var vel = delta;

			vel.Normalize();
			vel *= speed;

			var customProjectile = ProjectileFunctions.SpawnCustomProjectile(255, projectileName, start.X, start.Y, vel.X, vel.Y);
			//var customProjectile = ProjectileFunctions.SpawnCustomProjectile(owner, projectileName, start.X, start.Y, 20, 0);
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

		[Conditional("DEBUG")]
		public void DebugDropIn()
		{
			var firstPlayer = TShock.Players.FirstOrDefault();

			if( firstPlayer.Active )
			{
				var target = new Vector2(firstPlayer.X, firstPlayer.Y - 64);

				Teleport(target);

				//TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", Npc.whoAmI);
			}
		}

		public bool CustomIDContains(string text)
		{
			return this.Definition?.Name.Contains(text) == true;
		}

		public void AttachEmote(int emoteId, int lifeTime)
		{
			if( IsNpcValid )
			{
				EmoteFunctions.AttachEmote(EmoteFunctions.AnchorTypeNpc, this.Index, emoteId, lifeTime);
			}
		}

		#region ParentingSystem

		public CustomNpc Parent { get; set; }
		//private List<CustomNpc> children;
		public List<CustomNpc> Children { get; set; } = new List<CustomNpc>(); // => children ?? ( children = new List<CustomNpc>() );

		public bool HasParent => Parent != null;// && Parent.Npc.active;

		public bool HasChildren
		{
			get { return Children.Count>0; }
		}

		public bool IsParentRelative { get; set; } = true;
		public Vector2 ParentRelativePosition { get; set; }

		public void AttachChild(CustomNpc child)
		{
			Children.Add(child);
			child.Parent = this;
			child.Hp = Hp;
		}

		public void DetachChild(CustomNpc child)
		{
			if( child == null || child.Npc.active == false )
				return;

			var i = Children.IndexOf(child);

			if(i>-1)
				Children.RemoveAt(i);
			
			if( child.Parent == this )
				child.Parent = null;
		}

		public void UpdateChildren()
		{
			//if( !HasChildren )
			//	return;

			foreach(var ch in Children)
			{
				if( !ch.Npc.active )
					continue;

				if(ch.IsParentRelative)
				{
					ch.Position = Position + ch.ParentRelativePosition;
					ch.SendNetUpdate = true;
				}
			}
		}
		
		#endregion
	}

	public class Bone
	{
		
	}
}
