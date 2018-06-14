using CustomNpcsEdit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Controls
{
	public class ProjectileEditor : ObjectEditor
	{
		ProjectileBindingList projectiles;

		protected override void OnPostInitialize()
		{
			projectiles = new ProjectileBindingList();
			SetBindingCollection(projectiles);
		}

		protected override object OnCreateItem()
		{
			return new Projectile();
		}

		protected override object OnCopyItem(object source)
		{
			const string suffix = "(Copy)";

			var copy = new Projectile((Projectile)source);

			if( !copy.Name.EndsWith(suffix) )
				copy.Name = copy.Name + suffix;

			return copy;
		}

		protected override void OnFileLoad(string fileName)
		{
			projectiles.Clear();

			projectiles = ProjectileBindingList.Load(fileName);
			SetBindingCollection(projectiles);
		}

		protected override void OnFileSave(string fileName)
		{
			projectiles.Save(fileName);
		}
	}
}
