using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Queues and sends CombatTexts to players.
	/// </summary>
	public class CombatTextDistributor
	{
		ConcurrentQueue<CombatText> queue = new ConcurrentQueue<CombatText>();
		DateTime lastSendTime = DateTime.Now;

		public void AddCombatText(string text, TSPlayer player, Color color)
		{
			var combatText = new CombatText()
			{
				Text = text,
				Player = player,
				Color = color
			};

			queue.Enqueue(combatText);
		}

		public void Clear()
		{
			var count = queue.Count;

			for( var i = 0; i < count; i++ )
				queue.TryDequeue(out var result);
		}

		public void Send(int delayMS = 250)
		{
			var delaySpan = new TimeSpan(0, 0, 0, 0, delayMS);
			while( queue.Count > 0 )
			{
				var now = DateTime.Now;

				if( ( now - lastSendTime ) >= delaySpan && queue.Count>0)
				{
					if(queue.TryDequeue(out var combatText))
					{
						lastSendTime = now;
						combatText.Send();
					}
				}
				else
				{
					return;
				}
			}
		}
	}

	internal class CombatText
	{
		internal string Text { get; set; }
		internal TSPlayer Player { get; set; }
		internal Color Color { get; set; }
		internal bool IsGlobal { get; set; }
		
		internal void Send()
		{
			var tplayer = Player.TPlayer;
			
			if( IsGlobal )
				TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended, Text, (int)Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
			else
				Player.SendData(PacketTypes.CreateCombatTextExtended, Text, (int)Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
		}
	}
}
