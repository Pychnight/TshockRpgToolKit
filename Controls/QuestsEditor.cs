using RpgToolsEditor.Models;
using RpgToolsEditor.Models.CustomQuests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	public class QuestsEditor : ObjectEditor
	{
		protected override object OnCreateItem()
		{
			return new QuestInfo();
		}

		protected override object OnCopyItem(object source)
		{
			const string suffix = "(Copy)";

			var copy = new QuestInfo((QuestInfo)source);

			if( !copy.Name.EndsWith(suffix) )
				copy.Name = copy.Name + suffix;

			return copy;
		}

		protected override void OnFileLoad(string fileName)
		{
			var boundTreeNodes = ModelTreePersistance.LoadTree<QuestInfo>(fileName);
			SetTreeViewModels<IModel>(boundTreeNodes);

		}

		protected override void OnFileSave(string fileName)
		{
			var boundNodes = GetTreeViewModels();
			ModelTreePersistance.SaveTree(boundNodes, fileName);
		}
		
	}
}
