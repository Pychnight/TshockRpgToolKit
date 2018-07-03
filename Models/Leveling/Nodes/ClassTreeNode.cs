using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.Leveling
{
	public class ClassTreeNode : ModelTreeNode
	{
		public ClassTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public ClassTreeNode(Class model) : this()
		{
			Model = model;

			var levelsNode = new LevelsContainerTreeNode();
			//var itemsNode = new ItemsContainerTreeNode();

			//NpcShopCommandsContainerNode.Model = model.ShopCommands;
			//NpcShopItemsContainerNode.Model = model.ShopItems;
			Nodes.Add(levelsNode);
			//Nodes.Add(itemsNode);

			//commandsNode.AddChildModels(model.ShopCommands.Cast<IModel>().ToList());
			//itemsNode.AddChildModels(model.ShopItems.Cast<IModel>().ToList());
		}

		//public override void AddChild(ModelTreeNode node)
		//{
		//	base.AddChild(node);
		//}

		//public override ModelTreeNode AddItem()
		//{
		//	return base.AddItem();
		//}

		//public override void AddSibling(ModelTreeNode node)
		//{
		//	base.AddSibling(node);
		//}

		//public override bool CanAcceptDraggedNode(ModelTreeNode node)
		//{
		//	return base.CanAcceptDraggedNode(node);
		//}

		//public override object Clone()
		//{
		//	return base.Clone();
		//}

		//public override ModelTreeNode Copy()
		//{
		//	return base.Copy();
		//}

		//public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		//{
		//	return base.TryAcceptDraggedNode(draggedNode);
		//}

		//public override void TryDropWithNoTarget(TreeView treeView)
		//{
		//	base.TryDropWithNoTarget(treeView);
		//}
	}
}
