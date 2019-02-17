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

		/// <summary>
		///		Gets a value that determines whether the PartyMember is still connected to the party. 
		/// </summary>
		public bool IsValidMember { get; internal set; }
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

		public QuestStatusCollection QuestStatuses { get; internal set; } 

		internal PartyMember(TSPlayer player)
		{
			Player = player;
			Items = new PlayerInventoryManager(player);
			Variables = new Dictionary<string, object>();
			QuestStatuses = new QuestStatusCollection();
			
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
			QuestStatuses.Clear();
		}

		public void SetQuestStatus(int index, string text, Color color)
		{
			QuestStatuses.SetQuestStatus(index, text, color);
		}

		public void SetQuestStatus(int index, string text)
		{
			QuestStatuses.SetQuestStatus(index, text);
		}

		public void SetQuestStatus(string text, Color color)
		{
			SetQuestStatus(0, text, color);
		}

		public void SetQuestStatus(string text)
		{
			SetQuestStatus(0, text);
		}

		public QuestStatus GetQuestStatus(int index)
		{
			return QuestStatuses.GetQuestStatus(index);
		}

		public QuestStatus GetQuestStatus()
		{
			return GetQuestStatus(0);
		}
	}
}
