using System;

namespace CustomQuests
{
	public class CatchNpcEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int NpcId { get; private set; }

		internal CatchNpcEventArgs(int playerIndex, int npcId)
		{
			PlayerIndex = playerIndex;
			NpcId = npcId;
		}
	}
}
