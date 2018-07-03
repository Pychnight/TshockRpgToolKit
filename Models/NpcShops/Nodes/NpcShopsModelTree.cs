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
			var node = tree.FirstOrDefault() as NpcShopTreeNode;

			var shopModel = (NpcShop)node.Model;
			var shopCommands = getCommandModels(node);
			var shopItems = getItemModels(node);

			shopModel.ShopCommands = shopCommands;
			shopModel.ShopItems = shopItems;
						
			var json = JsonConvert.SerializeObject(shopModel, Formatting.Indented);
			File.WriteAllText(path, json);
		}
		
		private List<ShopCommand> getCommandModels(NpcShopTreeNode root)
		{
			var result = new List<ShopCommand>();
			var commandNodes = root.ShopCommandNodes.Nodes.Cast<CommandTreeNode>();

			foreach( var commandNode in commandNodes )
			{
				var command = (ShopCommand)commandNode.Model;

				command.RequiredItems.Clear();

				var requiredItems = commandNode.RequiredItemsTreeNode.Nodes.Cast<RequiredItemTreeNode>()
																			.Select(n => (RequiredItem)n.Model);


				command.RequiredItems.AddRange(requiredItems);

				result.Add(command);
			}

			return result;
		}

		private List<ShopItem> getItemModels(NpcShopTreeNode root)
		{
			var result = new List<ShopItem>();
			var itemNodes = root.ShopItemNodes.Nodes.Cast<ItemTreeNode>();

			foreach( var itemNode in itemNodes )
			{
				var command = (ShopItem)itemNode.Model;

				command.RequiredItems.Clear();

				var requiredItems = itemNode.RequiredItemsTreeNode.Nodes.Cast<RequiredItemTreeNode>()
																			.Select(n => (RequiredItem)n.Model);

				command.RequiredItems.AddRange(requiredItems);

				result.Add(command);
			}

			return result;
		}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new NpcShop();
			return new NpcShopTreeNode(item);
		}
	}
}
