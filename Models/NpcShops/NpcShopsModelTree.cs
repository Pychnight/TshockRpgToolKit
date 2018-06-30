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

namespace RpgToolsEditor.Models.NpcShops
{
	public class NpcShopsModelTree : ModelTree
	{
		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var item = new NpcShop();
			var node = new NpcShopTreeNode(item);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var item = JsonConvert.DeserializeObject<NpcShop>(json);

			//var nodes = items.Select(i => (ModelTreeNode)new NpcShopTreeNode(i)).ToList();

			var node = new List<ModelTreeNode>()
			{
				(ModelTreeNode)new NpcShopTreeNode(item)
			};

			return node;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var items = tree.Select(n => (NpcShop)n.Model).FirstOrDefault();

			var json = JsonConvert.SerializeObject(items, Formatting.Indented);
			File.WriteAllText(path, json);
		}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new NpcShop();
			return new NpcShopTreeNode(item);
		}
	}

	public class NpcShopTreeNode : ModelTreeNode
	{
		public NpcShopTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = false;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			//dont create nodes in the default ctor... it trips up the treeview on Clone().
			//Nodes.Add(new NpcShopCommandsContainerTreeNode());
			//Nodes.Add(new NpcShopItemsContainerTreeNode());
		}

		public NpcShopTreeNode(NpcShop model) : this()
		{
			Model = model;

			//NpcShopCommandsContainerNode.Model = model.ShopCommands;
			//NpcShopItemsContainerNode.Model = model.ShopItems;
			Nodes.Add(new NpcShopCommandsContainerTreeNode());
			Nodes.Add(new NpcShopItemsContainerTreeNode());
		}

		public NpcShopTreeNode(NpcShopTreeNode other)
		{
			
		}

		//public override ModelTreeNode Copy()
		//{
		//	var dstItem = new NpcShopDefinition((NpcShopDefinition)Model);
		//	var dstNode = new NpcShopTreeNode(dstItem);

		//	return dstNode;
		//}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is NpcShopTreeNode;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if(CanAcceptDraggedNode(draggedNode))
			{
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}

	public class NpcShopCommandsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public NpcShopCommandsContainerTreeNode()
		{
			Text = "ShopCommands";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override ModelTreeNode AddItem()
		{
			var item = new ShopCommand();
			var node = new NpcShopCommandTreeNode()
			{
				Model = item
			};

			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is NpcShopCommandTreeNode;
			return result;
		}

		//public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		//{
		//	if( !CanAcceptDraggedNode(draggedNode) )
		//		return false;

		//	return true;
		//}

	}

	public class NpcShopItemsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public NpcShopItemsContainerTreeNode() : base()
		{
			Text = "ShopItems";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override ModelTreeNode AddItem()
		{
			var item = new ShopItem();
			var node = new NpcShopItemTreeNode()
			{
				Model = item
			};

			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is NpcShopItemTreeNode;
			return result;
		}
	}

	public abstract class NpcShopProductTreeNode : ModelTreeNode
	{
		public NpcShopProductTreeNode()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}
		
		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//do nothing, this is not allowed for shop products.
		}
	}

	public class NpcShopCommandTreeNode : NpcShopProductTreeNode
	{
		public NpcShopCommandTreeNode() : base()
		{
		}

		public override ModelTreeNode AddItem()
		{
			var model = new ShopCommand();
			var node = new NpcShopCommandTreeNode();
			node.Model = model;
			
			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is NpcShopCommandTreeNode;
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

	public class NpcShopItemTreeNode : NpcShopProductTreeNode
	{
		public NpcShopItemTreeNode() : base()
		{
		}

		public override ModelTreeNode AddItem()
		{
			var model = new ShopItem();
			var node = new NpcShopItemTreeNode();
			node.Model = model;

			AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is NpcShopItemTreeNode;
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
