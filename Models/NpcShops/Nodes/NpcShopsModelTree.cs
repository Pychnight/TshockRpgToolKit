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
			var directory = Path.GetDirectoryName(path);
			var folderTreeNode = new FolderTreeNode();
			var shopPaths = Directory.EnumerateFiles(directory, "*.shop");

			folderTreeNode.Text = directory;

			foreach(var filePath in shopPaths)
			{
				var json = File.ReadAllText(filePath);
				var item = JsonConvert.DeserializeObject<NpcShop>(json);
				item.Filename = Path.GetFileName(filePath);
				var node = new NpcShopTreeNode(item);
				
				folderTreeNode.Nodes.Add(node);
			}

			folderTreeNode.Expand();
			
			return new List<ModelTreeNode>() { folderTreeNode };
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var directory = Path.GetDirectoryName(path);
			var folderNode = tree.FirstOrDefault() as FolderTreeNode;

			foreach(var node in folderNode.Nodes)
			{
				var shopNode = (NpcShopTreeNode)node;
				var shopModel = (NpcShop)shopNode.Model;
				var shopCommands = getCommandModels(shopNode);
				var shopItems = getItemModels(shopNode);

				shopModel.ShopCommands = shopCommands;
				shopModel.ShopItems = shopItems;

				var shopPath = Path.Combine(directory,shopModel.Filename);
				
				var json = JsonConvert.SerializeObject(shopModel, Formatting.Indented);
				File.WriteAllText(shopPath, json);
			}
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
