using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RpgToolsEditor.Models.Banking
{
	public class BankingModelTree : ModelTree
	{
		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var item = new CurrencyDefinition();
			var node = new CurrencyTreeNode(item);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var directory = Path.GetDirectoryName(path);
			var folderTreeNode = new FolderTreeNode();
			var shopPaths = Directory.EnumerateFiles(directory, "*.currency");

			folderTreeNode.Text = directory;

			foreach (var filePath in shopPaths)
			{
				var json = File.ReadAllText(filePath);
				var item = JsonConvert.DeserializeObject<CurrencyDefinition>(json);
				item.Filename = Path.GetFileName(filePath);
				var node = new CurrencyTreeNode(item);

				folderTreeNode.Nodes.Add(node);
			}

			folderTreeNode.Expand();

			return new List<ModelTreeNode>() { folderTreeNode };
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var directory = Path.GetDirectoryName(path);
			var folderNode = tree.FirstOrDefault() as FolderTreeNode;

			//duplicate name safeguard
			var shopModels = folderNode.Nodes.Cast<CurrencyTreeNode>().Select(n => (CurrencyDefinition)n.Model);

			shopModels.ThrowOnDuplicateNames();

			//save
			foreach (var node in folderNode.Nodes)
			{
				var currencyNode = (CurrencyTreeNode)node;
				var shopModel = (CurrencyDefinition)currencyNode.Model;
				//var shopCommands = getCommandModels(currencyNode);
				//var shopItems = getItemModels(currencyNode);

				//shopModel.ShopCommands = shopCommands;
				//shopModel.ShopItems = shopItems;

				var shopPath = Path.Combine(directory, shopModel.Filename);

				var json = JsonConvert.SerializeObject(shopModel, Formatting.Indented);
				File.WriteAllText(shopPath, json);
			}
		}

		//private List<ShopCommand> getCommandModels(CurrencyTreeNode root)
		//{
		//	var result = new List<ShopCommand>();
		//	var commandNodes = root.ShopCommandNodes.Nodes.Cast<CommandTreeNode>();

		//	foreach( var commandNode in commandNodes )
		//	{
		//		var command = (ShopCommand)commandNode.Model;

		//		command.RequiredItems.Clear();

		//		var requiredItems = commandNode.RequiredItemsTreeNode.Nodes.Cast<RequiredItemTreeNode>()
		//																	.Select(n => (RequiredItem)n.Model);


		//		command.RequiredItems.AddRange(requiredItems);

		//		result.Add(command);
		//	}

		//	return result;
		//}

		//private List<ShopItem> getItemModels(CurrencyTreeNode root)
		//{
		//	var result = new List<ShopItem>();
		//	var itemNodes = root.ShopItemNodes.Nodes.Cast<ItemTreeNode>();

		//	foreach( var itemNode in itemNodes )
		//	{
		//		var command = (ShopItem)itemNode.Model;

		//		command.RequiredItems.Clear();

		//		var requiredItems = itemNode.RequiredItemsTreeNode.Nodes.Cast<RequiredItemTreeNode>()
		//																	.Select(n => (RequiredItem)n.Model);

		//		command.RequiredItems.AddRange(requiredItems);

		//		result.Add(command);
		//	}

		//	return result;
		//}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new CurrencyDefinition();
			return new CurrencyTreeNode(item);
		}
	}
}
