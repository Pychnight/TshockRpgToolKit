using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Quests
{
	public class QuestStatusManager : IEnumerable<QuestStatus>
	{
		Dictionary<int, QuestStatus> items;

		public int Count => items.Count;

		internal QuestStatusManager()
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

		public QuestStatus GetQuestStatus(int index)
		{
			if( !items.TryGetValue(index, out var questStatus) )
			{
				//questStatus = new QuestStatus();
				//items.Add(index, questStatus);
			}

			return questStatus;
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
