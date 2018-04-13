using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI.Localization;

namespace CustomQuests.Next
{
	//some seemingly redundant overloads live in here, but are needed since boo doesnt support optional arguments.

	public static class PartyMemberExtensions
	{
		#region Communication

		public static void SendData(this PartyMember member, PacketTypes packetType, string text = "", int number = 0, float number2 = 0,
			float number3 = 0, float number4 = 0, int number5 = 0)
		{
			member.Player.SendData(packetType, text, number, number2, number3, number4, number5);
		}

		/// <summary>
		///     Sends data to the party.
		/// </summary>
		/// <param name="packetType">The packet type.</param>
		/// <param name="text">The text.</param>
		/// <param name="number">The first number.</param>
		/// <param name="number2">The second number.</param>
		/// <param name="number3">The third number.</param>
		/// <param name="number4">The fourth number.</param>
		/// <param name="number5">The fifth number.</param>
		public static void SendData(this IEnumerable<PartyMember> members, PacketTypes packetType, string text = "", int number = 0, float number2 = 0,
			float number3 = 0, float number4 = 0, int number5 = 0)
		{
			foreach( var m in members )
				m.SendData(packetType, text, number, number2, number3, number4, number5);
		}

		public static void SendErrorMessage(this PartyMember member, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.Player.SendErrorMessage(message);
		}

		/// <summary>
		///     Sends an error message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendErrorMessage(this IEnumerable<PartyMember> members, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var m in members )
				m.SendErrorMessage(message);
		}

		public static void SendWarningMessage(this PartyMember member, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.Player.SendWarningMessage(message);
		}

		/// <summary>
		///     Sends a warning message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendWarningMessage(this IEnumerable<PartyMember> members, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var m in members )
				m.SendWarningMessage(message);
		}

		public static void SendInfoMessage(this PartyMember member, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.Player.SendInfoMessage(message);
		}

		/// <summary>
		///     Sends an informational message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendInfoMessage(this IEnumerable<PartyMember> members, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var member in members )
				member.SendInfoMessage(message);
		}

		public static void SendMessage(this PartyMember member, string message, Color color)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.Player.SendMessage(message, color);
		}

		/// <summary>
		///     Sends a message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="color">The color.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendMessage( this IEnumerable<PartyMember> members, string message, Color color)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var m in members )
				m.SendMessage(message, color);
		}

		public static void SendMessage(this PartyMember member, string message, byte r, byte g, byte b)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.Player.SendMessage(message, r, g, b);
		}

		/// <summary>
		///     Sends a message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendMessage(this IEnumerable<PartyMember> members, string message, byte r, byte g, byte b)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var m in members )
				m.SendMessage(message, r, g, b);
		}

		public static void SendMessage(this PartyMember member, string message)
		{
			var color = Color.White;
			member.Player.SendMessage(message, color);
		}

		public static void SendMessage(this IEnumerable<PartyMember> members, string message)
		{
			foreach( var m in members )
				m.SendMessage(message);
		}

		public static void SendStatus(this PartyMember member, string status)
		{
			if( status == null )
				throw new ArgumentNullException(nameof(status));

			var text = status + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
								"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
			member.Player.SendData(PacketTypes.Status, text);
		}

		/// <summary>
		///     Sends a status to the party.
		/// </summary>
		/// <param name="status">The status, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="status" /> is <c>null</c>.</exception>
		public static void SendStatus(this IEnumerable<PartyMember> members, string status)
		{
			if( status == null )
				throw new ArgumentNullException(nameof(status));
			
			foreach( var m in members )
				m.SendStatus(status);
		}

		public static void SendSuccessMessage(this PartyMember member, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));

			member.SendSuccessMessage(message);
		}

		/// <summary>
		///     Sends a success message to the party.
		/// </summary>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
		public static void SendSuccessMessage(this IEnumerable<PartyMember> members, string message)
		{
			if( message == null )
				throw new ArgumentNullException(nameof(message));
			
			foreach( var m in members )
				m.SendSuccessMessage(message);
		}
		
		#endregion

		#region Status effects

		public static void Buff(this PartyMember member, int buffType, int seconds)
		{
			if( seconds <= 0 )
				throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
			
			member.Player.SetBuff(buffType, seconds * 60);
		}

		/// <summary>
		///     Buffs the party with the specified type.
		/// </summary>
		/// <param name="buffType">The type.</param>
		/// <param name="seconds">The number of seconds, which must be positive.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds" /> is not positive.</exception>
		public static void Buff(this IEnumerable<PartyMember> members, int buffType, int seconds)
		{
			if( seconds <= 0 )
					throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
			
			foreach( var member in members )
				member.Buff(buffType, seconds);
		}

		public static void Damage(this PartyMember member, int damage)
		{
			if( damage <= 0 )
				throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be positive.");
			
			member.Player.DamagePlayer(damage);
		}

		/// <summary>
		///     Damages the party.
		/// </summary>
		/// <param name="damage">The damage, which must be positive.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="damage" /> is not positive.</exception>
		public static void Damage(this IEnumerable<PartyMember> members, int damage)
		{
			if( damage <= 0 )
				throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be positive.");
			
			foreach( var member in members )
				member.Damage(damage);
		}

		public static void Heal(this PartyMember member, int health)
		{
			if( health <= 0 )
				throw new ArgumentOutOfRangeException(nameof(health), "Health must be positive.");
						
			member.Player.Heal(health);
		}

		/// <summary>
		///     Heals the party.
		/// </summary>
		/// <param name="health">The health, which must be positive.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="health" /> is not positive.</exception>
		public static void Heal(this IEnumerable<PartyMember> members, int health)
		{
			if( health <= 0 )
				throw new ArgumentOutOfRangeException(nameof(health), "Health must be positive.");
			
			foreach( var member in members )
				member.Heal(health);
		}

		#endregion

		#region Inventory

		private static int? GetItemIdFromName(string name)
		{
			for( var i = 0; i < Main.maxItemTypes; ++i )
			{
				var itemName = EnglishLanguage.GetItemNameById(i);
				if( itemName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false )
				{
					return i;
				}
			}
			return null;
		}

		public static void GiveItem(this PartyMember member, string name)
		{
			member.GiveItem(name, 1, 0);
		}

		public static void GiveItem(this PartyMember member, string name, int stackSize = 1, byte prefix = 0)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}
			if( stackSize <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
			}
			if( prefix > PrefixID.Count )
			{
				throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be within range.");
			}

			var itemId = GetItemIdFromName(name);
			if( itemId == null )
			{
				throw new FormatException($"Invalid item name '{name}'.");
			}

			member.Player.GiveItem((int)itemId, "", 20, 42, stackSize, prefix);
		}

		public static void GiveItem(this IEnumerable<PartyMember> members, string name)
		{
			members.GiveItem(name, 1, 0);
		}

		/// <summary>
		///     Gives the specified item to the party.
		/// </summary>
		/// <param name="name">The name, which must not be <c>null</c>.</param>
		/// <param name="stackSize">The stack size, which must be positive.</param>
		/// <param name="prefix">The prefix, which must be within range.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="stackSize" /> is not positive or <paramref name="prefix" /> is too large.
		/// </exception>
		public static void GiveItem(this IEnumerable<PartyMember> members, string name, int stackSize = 1, byte prefix = 0)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}
			if( stackSize <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
			}
			if( prefix > PrefixID.Count )
			{
				throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be within range.");
			}

			var itemId = GetItemIdFromName(name);
			if( itemId == null )
			{
				throw new FormatException($"Invalid item name '{name}'.");
			}

			foreach( var m in members )
			{
				//m.GiveItem((int)itemId, "", 20, 42, stackSize, prefix);
				m.Player.GiveItem((int)itemId, "", 20, 42, stackSize, prefix);
			}
		}
				
		public static void RemoveItem(this PartyMember member, string name)
		{
			member.RemoveItem(name, 1, 0);
		}

		public static void RemoveItem(this PartyMember member, string name, int stackSize = 1, byte prefix = 0)
		{
			var members = new PartyMember[] { member };
			members.RemoveItem(name, stackSize, prefix);
		}

		public static void RemoveItem(this IEnumerable<PartyMember> members, string name)
		{
			members.RemoveItem(name, 1, 0);
		}

		/// <summary>
		///     Removes the specified item from the party. This requires SSC to work.
		/// </summary>
		/// <param name="name">The name, which must not be <c>null</c>.</param>
		/// <param name="stackSize">The stack size, which must be positive.</param>
		/// <param name="prefix">The prefix, which must be within range.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="stackSize" /> is not positive or <paramref name="prefix" /> is too large.
		/// </exception>
		public static void RemoveItem(this IEnumerable<PartyMember> members, string name, int stackSize = 1, byte prefix = 0)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}
			if( stackSize <= 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
			}
			if( prefix > PrefixID.Count )
			{
				throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be within range.");
			}

			var itemId = GetItemIdFromName(name);
			if( itemId == null )
			{
				throw new FormatException($"Invalid item name '{name}'.");
			}

			foreach( var m in members )
			{
				var required = stackSize;
				var offset = 0;

				void Check(IReadOnlyList<Item> items)
				{
					for( var i = 0; i < items.Count && required > 0; ++i )
					{
						var item = items[i];
						if( item.type == itemId && item.prefix == prefix )
						{
							var loss = Math.Min(item.stack, required);
							item.stack -= loss;
							required -= loss;
							m.SendData(PacketTypes.PlayerSlot, "", m.Player.Index, offset + i);
						}
					}
					offset += items.Count;
				}

				var tplayer = m.Player.TPlayer;
				Check(tplayer.inventory);
				Check(tplayer.armor);
				Check(tplayer.dye);
				Check(tplayer.miscEquips);
				Check(tplayer.miscDyes);
				Check(tplayer.bank.item);
				Check(tplayer.bank2.item);
				Check(new[] { tplayer.trashItem });
				Check(tplayer.bank3.item);
			}
		}

		#endregion

		#region Others

		public static void Teleport(this PartyMember member, int x, int y)
		{
			member.Player.Teleport(16 * x, 16 * y);
		}

		/// <summary>
		///     Teleports the party to the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		public static void Teleport(this IEnumerable<PartyMember> members, int x, int y)
		{
			foreach( var m in members )
				m.Teleport(16 * x, 16 * y);
		}

		#endregion
	}
}
