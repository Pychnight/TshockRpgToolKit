using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.NpcShops
{
	public class ItemTreeNode : ProductTreeNode
	{
		public ItemTreeNode() : base()
		{
			ImageIndex = SelectedImageIndex = 3;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new ShopItem();
			var node = new ItemTreeNode();
			node.AddDefaultChildNodesHack();
			node.Model = model;
			
			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is ItemTreeNode;
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
	}
}
