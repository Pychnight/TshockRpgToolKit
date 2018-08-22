using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.Banking
{
	public class CurrencyTreeNode : ModelTreeNode
	{
		//public CommandsContainerTreeNode ShopCommandNodes => Nodes[0] as CommandsContainerTreeNode;
		//public ItemsContainerTreeNode ShopItemNodes => Nodes[1] as ItemsContainerTreeNode;

		public CurrencyTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = false;
			CanCopy = false;
			CanDelete = true;
			CanDrag = true;

			//dont create nodes in the default ctor... it trips up the treeview on Clone().
			//Nodes.Add(new NpcShopCommandsContainerTreeNode());
			//Nodes.Add(new NpcShopItemsContainerTreeNode());
		}

		public CurrencyTreeNode(CurrencyDefinition model) : this()
		{
			Model = model;

			//quadrants node

			//npcs

			//mining

			//placing

			//fishing

			//var commandsNode = new CommandsContainerTreeNode();
			//var itemsNode = new ItemsContainerTreeNode();

			//NpcShopCommandsContainerNode.Model = model.ShopCommands;
			//NpcShopItemsContainerNode.Model = model.ShopItems;
			//Nodes.Add(commandsNode);
			//Nodes.Add(itemsNode);

			//commandsNode.AddChildModels(model.ShopCommands.Cast<IModel>().ToList());
			//itemsNode.AddChildModels(model.ShopItems.Cast<IModel>().ToList());

		}

		public CurrencyTreeNode(CurrencyTreeNode other)
		{
			throw new NotImplementedException();
		}

		//public override ModelTreeNode Copy()
		//{
		//	var dstItem = new NpcShopDefinition((NpcShopDefinition)Model);
		//	var dstNode = new NpcShopTreeNode(dstItem);

		//	return dstNode;
		//}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			//return node is CurrencyTreeNode;
			return false;
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

		//use when no node was found as a drop target... ie, dropping on the TreeView itself. 
		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//var parent = this.Parent;

			this.Remove();
			treeView.Nodes.Add(this);

			var folderNode = (FolderTreeNode)treeView.Nodes[0];

			folderNode.AddChild(this);

			//not sure how to resolve updating dirty status for now...
			//IsTreeDirty = true;
		}
	}
}
