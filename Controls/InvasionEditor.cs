using CustomNpcsEdit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Controls
{
	public class InvasionEditor : ObjectEditor
	{
		InvasionBindingList invasions;

		protected override void OnPostInitialize()
		{
			invasions = new InvasionBindingList();
			SetBindingCollection(invasions);
		}

		protected override object OnCreateItem()
		{
			return new Invasion();
		}

		protected override object OnCopyItem(object source)
		{
			const string suffix = "(Copy)";

			var copy = new Invasion((Invasion)source);

			if( !copy.Name.EndsWith(suffix) )
				copy.Name = copy.Name + suffix;

			return copy;
		}

		protected override void OnFileLoad(string fileName)
		{
			invasions.Clear();

			invasions = InvasionBindingList.Load(fileName);
			SetBindingCollection(invasions);
		}

		protected override void OnFileSave(string fileName)
		{
			invasions.Save(fileName);
		}
	}
}
