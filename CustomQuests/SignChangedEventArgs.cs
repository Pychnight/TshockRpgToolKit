using System;

namespace CustomQuests
{
	public class SignChangedEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int SignId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public string Text { get; private set; }

		public SignChangedEventArgs(int playerId, int signId, int x, int y, string text)
		{
			PlayerIndex = playerId;
			SignId = signId;
			X = x;
			Y = y;
			Text = text;
		}
	}
}