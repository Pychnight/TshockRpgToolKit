using CustomNpcs.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Models
{
	//public abstract class Context<TItem>
	//{
	//	public string Filename { get; set; }


	//	public abstract Context<TItem> Load(string fileName);
	//}

	/// <summary>
	/// Wraps a CustomNpcs.ProjectileDefinition, and provides a shape suitable for our editor.
	/// </summary>
	public class Projectile
	{
		internal ProjectileDefinition WrappedObject { get; set; } 
	}

	public class ProjectileContext : List<Projectile>
	{
		internal static ProjectileContext CreateMockContext()
		{
			var result = new ProjectileContext();

			for(var i =0;i<3;i++)
			{
				result.Add(new Projectile());
			}
			
			return result;
		}
	}
}
