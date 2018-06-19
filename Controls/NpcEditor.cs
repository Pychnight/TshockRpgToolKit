using CustomNpcsEdit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Controls
{
	public class NpcEditor : ObjectEditor
	{
		//NpcTree npcTree; 
		NpcBindingList npcs;
		
		protected override void OnPostInitialize()
		{
			npcs = new NpcBindingList();
			SetBindingCollection(npcs);
		}

		protected override object OnCreateItem()
		{
			return new Npc();
		}

		protected override object OnCopyItem(object source)
		{
			const string suffix = "(Copy)";

			var copy = new Npc((Npc)source);

			if( !copy.Name.EndsWith(suffix) )
				copy.Name = copy.Name + suffix;

			return copy;
		}

		protected override void OnFileLoad(string fileName)
		{
			npcs.Clear();
			npcs = NpcBindingList.Load(fileName);
			SetBindingCollection(npcs);

			//--------
			var boundTreeNodes = NpcTree.LoadTree<Npc>(fileName);
			SetTreeViewModels<IModel>(boundTreeNodes);

		}

		protected override void OnFileSave(string fileName)
		{
			npcs.Save(fileName);
		}
	}
}
