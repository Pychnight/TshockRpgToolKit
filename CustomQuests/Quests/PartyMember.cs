using Corruption;
using CustomQuests.Sessions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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

		public Team Team { get; internal set; }
		public bool HasTeam => Team != null;

		public int SpawnTileX { get; set; }
		public int SpawnTileY { get; set; }
		
		public PlayerInventoryManager Items { get; private set; }

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
			Items = new PlayerInventoryManager(player);
			Variables = new Dictionary<string, object>();

			SpawnTileX = Main.spawnTileX;
			SpawnTileY = Main.spawnTileY;
		}

		public override string ToString()
		{
			return Player?.ToString();
		}

		public IEnumerable<PartyMember> ToEnumerable()
		{
			return new PartyMember[] { this };
		}

		public Session GetSession()
		{
			return CustomQuestsPlugin.Instance.GetSession(this);
		}

		public void ClearQuestStatus()
		{
			var session = GetSession();
			session.QuestStatusManager.Clear();
		}

		public void SetQuestStatus(int index, string text, Color color)
		{
			var session = GetSession();
			session.QuestStatusManager.SetQuestStatus(index, text, color);
		}

		public void SetQuestStatus(int index, string text)
		{
			var session = GetSession();
			session.QuestStatusManager.SetQuestStatus(index, text);
		}

		public QuestStatus GetQuestStatus(int index)
		{
			var session = GetSession();
			return session.QuestStatusManager.GetQuestStatus(index);
		}

		public IEnumerable<QuestStatus> GetQuestStatus()
		{
			var session = GetSession();
			return session.QuestStatusManager;
		}
	}
}
