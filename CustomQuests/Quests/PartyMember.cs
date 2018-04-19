using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	public class PartyMember
	{
		public TSPlayer Player { get; private set; }
		public string Name => Player.Name;
		public int Index => Player.Index;
		public float X => Player.X;
		public float Y => Player.Y;
		public int TileX => Player.TileX;
		public int TileY => Player.TileY;

		public Dictionary<string,object> Variables { get; private set; }
		public object this[string variableName]
		{
			get
			{
				Variables.TryGetValue(variableName, out var varValue);
				return varValue;
			}
			set
			{
				Variables[variableName] = value;
			}
		}

		public PartyMember(TSPlayer player)
		{
			Player = player;
			Variables = new Dictionary<string, object>();
		}

		public override string ToString()
		{
			return Player?.ToString();
		}

		public IEnumerable<PartyMember> ToEnumerable()
		{
			return new PartyMember[] { this };
		}
	}
}
