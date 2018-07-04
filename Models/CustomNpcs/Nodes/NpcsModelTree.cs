using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class NpcsModelTree : ModelTree
	{
		public override ModelTreeNode CreateDefaultItem()
		{
			var model = new Npc();
			var node = new NpcTreeNode(model);
			return node;
		}

		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var model = new Npc();
			var node = new NpcTreeNode(model);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var projectiles = JsonConvert.DeserializeObject<List<Npc>>(json);
			var nodes = projectiles.Select(p => (ModelTreeNode)new NpcTreeNode(p)).ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = new List<Npc>();
			
			foreach(var node in tree)
			{
				if(node is NpcTreeNode)
				{
					var npcTreeNode = (NpcTreeNode)node;
					var npc = npcTreeNode.Model as Npc;
					var lootEntries = npcTreeNode.LootEntryContainerNode.GetChildModels()
																		.Cast<LootEntry>()
																		.ToList();

					npc.LootEntries = lootEntries;
					models.Add(npc);
				}
				//else if(node is CategoryTreeNode)
				//{

				//}
			}
			
			var json = JsonConvert.SerializeObject(models);
			File.WriteAllText(path, json);
		}
	}
}