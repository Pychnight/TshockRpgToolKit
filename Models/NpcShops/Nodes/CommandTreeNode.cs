using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.NpcShops
{
	public class CommandTreeNode : ProductTreeNode
	{
		public CommandTreeNode() : base()
		{
			ImageIndex = SelectedImageIndex = 4;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new ShopCommand();
			var node = new CommandTreeNode();
			node.AddDefaultChildNodesHack();
			
			node.Model = model;

			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is CommandTreeNode;
			return result;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( !CanAcceptDraggedNode(draggedNode) )
				return false;

			draggedNode.Remove();
			AddSibling(draggedNode);
			this.Expand();

			return true;
		}
	}
}
