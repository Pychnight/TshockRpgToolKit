using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{
	public class InvasionBindingList : BindingList<Invasion>
	{
		internal static InvasionBindingList CreateMockContext()
		{
			var result = new InvasionBindingList();

			for( var i = 0; i < 3; i++ )
			{
				result.Add(new Invasion()
				{
					Name = $"Projectile{i}",
					ScriptPath = @"scripts/basicprojectile.boo"
				});
			}

			return result;
		}

		public static InvasionBindingList Load(string fileName)
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			var items = JsonConvert.DeserializeObject<InvasionBindingList>(json);

			return items;
		}

		internal void Save(string fileName)
		{
			throw new NotImplementedException("Saving is disabled for Invasions currently.");

			var json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(fileName, json);
		}
	}
}
