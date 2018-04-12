using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Next
{
	public class PartyMember
	{
		public TSPlayer Player { get; private set; }
		public string Name => Player.Name;

		public PartyMember(TSPlayer player)
		{
			Player = player;
		}
	}
}
