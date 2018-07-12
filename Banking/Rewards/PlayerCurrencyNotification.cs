using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Stores currency gains or losses for a player, in order to be sent as a CombatText.
	/// </summary>
	internal class PlayerCurrencyNotification
	{
		internal DateTime TimeStamp { get; set; }
		internal TSPlayer Player { get; set; }
		internal Color Color { get; set; }
		internal bool IsGlobal { get; set; }
		internal decimal Value { get; set; }
		internal CurrencyDefinition CurrencyDefinition { get; set; }

		private int debugAccumulateCounter = 1;

		internal void Accumulate(PlayerCurrencyNotification other)
		{
			Debug.Assert(Player == other.Player &&
						IsGlobal == other.IsGlobal &&
						CurrencyDefinition == other.CurrencyDefinition,
						"Attempted to accumulate 2 PlayerCurrencyNotifications, but they differ in type.");

			Value += other.Value;

			debugAccumulateCounter++;
		}

		private string createCurrencyText()
		{
			Color color = Color.White;
			var money = CurrencyDefinition.GetCurrencyConverter().ToStringAndColor(Value, ref color, QuadDisplayFormat.Abbreviation, useCommas: false, isCombatText: true);
			var text = $"{money}";

			Color = color;

			Debug.Print($"Accumulated {debugAccumulateCounter} transactions. - {Player.Name} gained {text}. ( {Value} {CurrencyDefinition.InternalName} )");

			return text;
		}

		internal void Send()
		{
			var tplayer = Player.TPlayer;
			var text = createCurrencyText();

			if( IsGlobal )
				TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended, text, (int)Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
			else
				Player.SendData(PacketTypes.CreateCombatTextExtended, text, (int)Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
		}
	}
}
