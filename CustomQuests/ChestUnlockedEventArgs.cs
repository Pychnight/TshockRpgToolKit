using System;
using System.Diagnostics;

namespace CustomQuests
{
	public class ChestUnlockedEventArgs : EventArgs
	{
		public int ChestX { get; private set; }
		public int ChestY { get; private set; }

		internal ChestUnlockedEventArgs( int chestX, int chestY)
		{
			ChestX = chestX;
			ChestY = chestY;
		}
	}
}