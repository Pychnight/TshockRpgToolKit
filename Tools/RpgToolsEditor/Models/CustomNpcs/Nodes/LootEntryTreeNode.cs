using RpgToolsEditor.Controls;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class LootEntryTreeNode : ModelTreeNode
	{
		public LootEntryTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			ImageIndex = SelectedImageIndex = 3;
		}

		public LootEntryTreeNode(LootEntry model) : this()
		{
			Model = model;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new LootEntry();
			var node = new LootEntryTreeNode();
			node.AddDefaultChildNodesHack();
			node.Model = model;

			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is LootEntryTreeNode;
			return result;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( !CanAcceptDraggedNode(draggedNode) )
				return false;

			draggedNode.Remove();
			AddSibling(draggedNode);

			return true;
		}

		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//not allowed
		}
	}
}