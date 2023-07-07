using System;

namespace CustomQuests
{
	public class ChestItemChangedEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int ChestId { get; private set; }
		public int ItemSlot { get; private set; }
		public int Stack { get; private set; }
		public int Prefix { get; private set; }
		public int NetId { get; private set; }

		public ChestItemChangedEventArgs(int playerIndex, int chestId, int itemSlot, int stack, int prefix, int netId)
		{
			PlayerIndex = playerIndex;
			ChestId = chestId;
			ItemSlot = itemSlot;
			Stack = stack;
			Prefix = prefix;
			NetId = netId;
		}
	}
}