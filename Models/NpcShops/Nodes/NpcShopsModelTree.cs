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

			var commandModels = node.ShopCommandNodes.GetChildModels().Cast<ShopCommand>();
			var itemModels = node.ShopItemNodes.GetChildModels().Cast<ShopItem>();

			var shopModel = (NpcShop)node.Model;

			shopModel.ShopCommands.Clear();
			shopModel.ShopCommands.AddRange(commandModels);

			shopModel.ShopItems.Clear();
			shopModel.ShopItems.AddRange(itemModels);
			
			var json = JsonConvert.SerializeObject(shopModel, Formatting.Indented);
			File.WriteAllText(path, json);
		}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new NpcShop();
			return new NpcShopTreeNode(item);
		}
	}
}
