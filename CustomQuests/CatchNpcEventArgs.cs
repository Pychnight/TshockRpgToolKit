using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
