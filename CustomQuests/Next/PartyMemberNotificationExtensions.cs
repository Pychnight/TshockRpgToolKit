using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Next
{
	public static class PartyMemberNotificationExtensions
	{
		public static void SendMessage(this PartyMember member, string message)
		{
			//Console.WriteLine(message);
			var color = Color.White;
			member.Player.SendMessage(message, color);
		}

		public static void SendMessage(this IEnumerable<PartyMember> members, string message)
		{
			foreach( var m in members )
				m.SendMessage(message);
		}
	}
}
