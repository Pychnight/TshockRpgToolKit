using RpgToolsEditor.Controls;
using System.Linq;

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

			node.AddDefaultChildNodesHack();

			//set child models
			var shopItem = (ShopItem)model;
			node.RequiredItemsTreeNode.AddChildModels(shopItem.RequiredItems.Cast<IModel>().ToList());

			//add to tree
			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new ShopItem();
			var node = new ItemTreeNode()
			{
				Model = item
			};

			node.AddDefaultChildNodesHack();
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
