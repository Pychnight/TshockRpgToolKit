using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Quests
{
	public class QuestStatusCollection : IEnumerable<QuestStatus>
	{
		Dictionary<int, QuestStatus> items;

		public int Count => items.Count;

		internal QuestStatusCollection()
		{
			items = new Dictionary<int, QuestStatus>();
		}

		public void Clear()
		{
			items.Clear();
		}

		public void SetQuestStatus(int index, string text, Color color)
		{
			if( !items.TryGetValue(index, out var questStatus) )
			{
				questStatus = new QuestStatus();
				items.Add(index, questStatus);
			}

			questStatus.Text = text;
			questStatus.Color = color;
		}

		public void SetQuestStatus(int index, string text)
		{
			SetQuestStatus(index, text, Color.White);
		}

		public void SetQuestStatus(string text)
		{
			SetQuestStatus(0, text);
		}

		public QuestStatus GetQuestStatus(int index)
		{
			if( !items.TryGetValue(index, out var questStatus) )
			{
				if(index==0)
				{
					//create a default status, esp. for overloads that dont use indices. 
					questStatus = new QuestStatus();
					items.Add(0, questStatus);
				}
			}

			return questStatus;
		}

		public QuestStatus GetQuestStatus()
		{
			return GetQuestStatus(0);
		}

		public IEnumerator<QuestStatus> GetEnumerator()
		{
			var ordered = items.OrderBy(i => i.Key)
								.Select( i => i.Value);

			foreach( var q in ordered )
				yield return q;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
