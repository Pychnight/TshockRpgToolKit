using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.Linq;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class NpcTreeNode : ModelTreeNode
	{
		public LootEntrysContainerTreeNode LootEntryContainerNode => Nodes[0] as LootEntrysContainerTreeNode;

		public NpcTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public NpcTreeNode(Npc model) : this()
		{
			Model = model;

			//var container = new LootEntrysContainerTreeNode(model.loot);
			var container = new LootEntrysContainerTreeNode();

			container.AddChildModels(model.LootEntries.Cast<IModel>().ToList());

			Nodes.Add(container);
		}

		public override void AddDefaultChildNodesHack()
		{
			//var model = new LootDefinition();
			//var container = new LootEntrysContainerTreeNode(model);
			var container = new LootEntrysContainerTreeNode();

			container.AddChildModels(((Npc)Model).LootEntries.Cast<IModel>().ToList());

			Nodes.Add(container);
		}

		public override ModelTreeNode AddItem()
		{
			var model = new Npc();
			var node = new NpcTreeNode(model);

			this.AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is NpcTreeNode ||
					( node is CategoryTreeNode<Npc,NpcTreeNode> && Parent == null );//dont accept CategoryTreeNodes, if were not in the root treeview( else bad things happen ).
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( CanAcceptDraggedNode(draggedNode) )
			{
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}
}