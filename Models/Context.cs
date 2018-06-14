using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{
	public class Context<TItem>
	{
		public string Filename { get; set; }
		public BindingList<TItem> Items { get; set; }

		public virtual void Load(string fileName)
		{
			throw new NotImplementedException();
		}

		public virtual void Save(string fileName)
		{
			throw new NotImplementedException();
		}
	}

	public class ProjectileContext : BindingList<Projectile>
	{
		internal static ProjectileContext CreateMockContext()
		{
			var result = new ProjectileContext();

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

		public static ProjectileContext Load(string fileName)
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			var ctx = JsonConvert.DeserializeObject<ProjectileContext>(json);

			return ctx;
		}

		internal void Save(string fileName)
		{
			var json = JsonConvert.SerializeObject(this);

			File.WriteAllText(fileName, json);
		}
	}
}
