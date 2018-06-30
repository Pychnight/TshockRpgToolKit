using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.NpcShops
{
	public class NpcShopsModelTree : ModelTree
	{
		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var item = new NpcShopDefinition();
			var node = new NpcShopTreeNode(item);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var item = JsonConvert.DeserializeObject<NpcShopDefinition>(json);

			//var nodes = items.Select(i => (ModelTreeNode)new NpcShopTreeNode(i)).ToList();

			var node = new List<ModelTreeNode>()
			{
				(ModelTreeNode)new NpcShopTreeNode(item)
			};

			return node;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var items = tree.Select(n => (NpcShopDefinition)n.Model).FirstOrDefault();

			var json = JsonConvert.SerializeObject(items, Formatting.Indented);
			File.WriteAllText(path, json);
		}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new NpcShopDefinition();
			return new NpcShopTreeNode(item);
		}
	}

	public class NpcShopTreeNode : ModelTreeNode
	{
		public NpcShopTreeNode() : base()
		{
			CanEditModel = true;
			CanAddChild = false;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			//dont create nodes in the default ctor... it trips up the treeview on Clone().
			//Nodes.Add(new NpcShopCommandsContainerTreeNode());
			//Nodes.Add(new NpcShopItemsContainerTreeNode());
		}

		public NpcShopTreeNode(NpcShopDefinition model) : this()
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
	}

	public class NpcShopCommandsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public NpcShopCommandsContainerTreeNode()
		{
			Text = "ShopCommands";
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
	}

	public class NpcShopItemsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public NpcShopItemsContainerTreeNode() : base()
		{
			Text = "ShopItems";
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
	}

	public class NpcShopCommandTreeNode : ModelTreeNode
	{
		public NpcShopCommandTreeNode()
		{
			CanEditModel = true;
			CanAddChild = false;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		//public override ModelTreeNode Copy()
		//{
		//	var item = new ShopCommand((ShopCommand)Model);
		//	var node = new NpcShopCommandTreeNode()
		//	{
		//		Model = item
		//	};
			
		//	return node;
		//}
	}

	public class NpcShopItemTreeNode : ModelTreeNode
	{
		public NpcShopItemTreeNode()
		{
			CanEditModel = true;
			CanAddChild = false;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public override ModelTreeNode Copy()
		{
			var item = new ShopItem((ShopItem)Model);
			var node = new NpcShopItemTreeNode()
			{
				Model = item
			};

			return node;
		}
	}
}
