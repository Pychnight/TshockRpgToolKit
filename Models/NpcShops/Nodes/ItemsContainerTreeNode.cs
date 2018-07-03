using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.NpcShops
{
	public class ItemsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public ItemsContainerTreeNode() : base()
		{
			Text = "ShopItems";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override void AddChildModel(IModel model)
		{
			var node = new ItemTreeNode()
			{
				Model = model
			};

			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new ShopItem();
			var node = new ItemTreeNode()
			{
				Model = item
			};

			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is ItemTreeNode;
			return result;
		}
	}
}
