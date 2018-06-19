using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{
	public class NpcBindingList : BindingList<Npc>
	{
		internal static NpcBindingList CreateMockContext()
		{
			var result = new NpcBindingList();

			for( var i = 0; i < 3; i++ )
			{
				result.Add(new Npc()
				{
					Name = $"Npc{i}",
					ScriptPath = @"scripts/basicnpc.boo",
					BaseType = i
				});
			}

			return result;
		}

		public static NpcBindingList Load(string fileName)
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			var items = JsonConvert.DeserializeObject<NpcBindingList>(json);
			
			return items;
		}
		
		internal void Save(string fileName)
		{
			var json = JsonConvert.SerializeObject(this,Formatting.Indented);
			File.WriteAllText(fileName, json);
		}
	}
}
