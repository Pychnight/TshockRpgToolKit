using RpgToolsEditor.Controls;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class WaveTreeNode : ModelTreeNode
	{
		public WaveTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			ImageIndex = SelectedImageIndex = 3;
		}

		public WaveTreeNode(LootEntry model) : this()
		{
			Model = model;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new Wave();
			var node = new WaveTreeNode();
			node.AddDefaultChildNodesHack();
			node.Model = model;

			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is WaveTreeNode;
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