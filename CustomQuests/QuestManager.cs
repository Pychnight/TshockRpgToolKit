using CustomQuests.Quests;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests
{
	public class QuestManager : IEnumerable<QuestInfo>
	{
		List<QuestInfo> questInfoList;//maintains ordering, enables correct serialization/deserialization to json...
		Dictionary<string, QuestInfo> questInfos;

		public int Count => questInfoList.Count;
		public QuestInfo this[int index] => questInfoList[index];
		public QuestInfo this[string name]
		{
			get
			{
				questInfos.TryGetValue(name, out var qi);
				return qi;
			}
		}
		
		public QuestManager()
		{
			questInfos = new Dictionary<string, QuestInfo>();
		}

		public void LoadQuestInfos(string fileName)
		{
			try
			{
				if( File.Exists(fileName) )
				{
					var json = File.ReadAllText(fileName);
					questInfoList = JsonConvert.DeserializeObject<List<QuestInfo>>(json);
				}
				else
				{
					questInfoList = new List<QuestInfo>();
					var json = JsonConvert.SerializeObject(questInfoList);
					File.WriteAllText(fileName, json);
				}
			}
			catch(Exception ex)
			{
				Debug.Print(ex.Message);
				Debug.Print(ex.StackTrace);

				questInfoList = new List<QuestInfo>();
			}

			questInfos.Clear();
			foreach( var qi in questInfoList )
				questInfos.Add(qi.Name, qi);

			Debug.Print("Found the following quest infos:");
			foreach( var qi in questInfoList )
				Debug.Print($"Quest: {qi.Name},  {qi.FriendlyName} - {qi.Description}");
		}
		
		public IEnumerator<QuestInfo> GetEnumerator()
		{
			return questInfoList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return questInfoList.GetEnumerator();
		}
	}
}
