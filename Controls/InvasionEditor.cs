using CustomNpcsEdit.Models;
using CustomNpcsEdit.Models.CustomNpcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Controls
{
	public class InvasionEditor : ObjectEditor
	{
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
			var boundTreeNodes = ModelTreePersistance.LoadTree<Invasion>(fileName);
			SetTreeViewModels<IModel>(boundTreeNodes);
		}

		protected override void OnFileSave(string fileName)
		{
			var boundNodes = GetTreeViewModels();
			ModelTreePersistance.SaveTree(boundNodes, fileName);
		}
	}
}
