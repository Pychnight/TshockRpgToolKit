using System;

namespace CustomQuests
{
	public class ChestUnlockedEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int ChestX { get; private set; }
		public int ChestY { get; private set; }

		internal ChestUnlockedEventArgs(int playerIndex, int chestX, int chestY)
		{
			PlayerIndex = playerIndex;
			ChestX = chestX;
			ChestY = chestY;
		}
	}
}