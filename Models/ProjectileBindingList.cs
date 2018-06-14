using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{
	public class ProjectileBindingList : BindingList<Projectile>
	{
		internal static ProjectileBindingList CreateMockContext()
		{
			var result = new ProjectileBindingList();

			for(var i =0;i<3;i++)
			{
				result.Add(new Projectile()
				{
					Name = $"Projectile{i}",
					ScriptPath = @"scripts/basicprojectile.boo",
					BaseType = i
				});
			}
			
			return result;
		}

		public static ProjectileBindingList Load(string fileName)
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			var items = JsonConvert.DeserializeObject<ProjectileBindingList>(json);
			
			return items;
		}

		internal void Save(string fileName)
		{
			var json = JsonConvert.SerializeObject(this);
			File.WriteAllText(fileName, json);
		}
	}
}
