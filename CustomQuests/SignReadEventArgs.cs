using System;

namespace CustomQuests
{
	public class SignReadEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public SignReadEventArgs(int playerIndex, int x, int y)
		{
			PlayerIndex = playerIndex;
			X = x;
			Y = y;
		}
	}
}