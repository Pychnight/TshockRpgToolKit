using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.Leveling
{
	public class LevelTreeNode : ModelTreeNode
	{
		public LevelTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			ImageIndex = SelectedImageIndex = 3;
		}

		public LevelTreeNode(Level model) : this()
		{
			Model = model;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new Level();
			var node = new LevelTreeNode();
			node.AddDefaultChildNodesHack();
			node.Model = model;

			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is LevelTreeNode;
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
