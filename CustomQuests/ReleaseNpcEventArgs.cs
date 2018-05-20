using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests
{
	public class ReleaseNpcEventArgs : EventArgs
	{
		public int PlayerIndex { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public short NpcType { get; private set; }
		public byte Style { get; private set; }

		public ReleaseNpcEventArgs(int playerIndex, int x, int y, short npcType, byte style)
		{
			PlayerIndex = playerIndex;
			X = x;
			Y = y;
			NpcType = npcType;
			Style = style;
		}
	}
}
