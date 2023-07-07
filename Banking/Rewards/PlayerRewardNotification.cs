using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Stores currency gains or losses for a player, in order to be sent as a CombatText.
	/// </summary>
	internal class PlayerRewardNotification
	{
		internal DateTime TimeStamp { get; set; }
		internal TSPlayer Player { get; set; }
		Color Color;
		internal bool IsGlobal { get; set; }
		internal decimal Value { get; set; }
		internal CurrencyDefinition CurrencyDefinition { get; set; }
		//private int debugAccumulateCounter = 1;

		internal void Accumulate(PlayerRewardNotification other)
		{
			Debug.Assert(Player == other.Player &&
						IsGlobal == other.IsGlobal &&
						CurrencyDefinition == other.CurrencyDefinition,
						"Attempted to accumulate 2 PlayerCurrencyNotifications, but they differ in type.");

			Value += other.Value;
			//debugAccumulateCounter++;
		}

		private string createCurrencyText()
		{
			//dont send combat texts for fractional values
			if (Math.Truncate(Value) == 0)
			{
				//Debug.Print($"Accumulated {debugAccumulateCounter} transactions. - Not sending {Player.Name} CombatText for fractional value. ( {Value} {CurrencyDefinition.InternalName} )");
				return null;
			}
			else
			{
				var text = CurrencyDefinition.GetCurrencyConverter().ToStringAndColor(Value, ref Color);

				//Debug.Print($"Accumulated {debugAccumulateCounter} transactions. - {Player.Name} gained {text}. ( {Value} {CurrencyDefinition.InternalName} )");

				return text;
			}
		}

		internal void Send(float xOffset = 0f, float yOffset = 0f)
		{
			var tplayer = Player.TPlayer;
			var text = createCurrencyText();

			//dont send combat texts for fractional values.
			if (text == null)
				return;

			if (IsGlobal)
				TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended, text, (int)Color.PackedValue, tplayer.Center.X + xOffset, tplayer.Center.Y + yOffset);
			else
				Player.SendData(PacketTypes.CreateCombatTextExtended, text, (int)Color.PackedValue, tplayer.Center.X + xOffset, tplayer.Center.Y + yOffset);
		}
	}
}
