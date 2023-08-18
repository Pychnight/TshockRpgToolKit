using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			var loader = new CategoryLoader<Npc>();
			var models = loader.ParseCategorysAndModels(json);

			//var nodes = npcs.Select(p => (ModelTreeNode)new NpcTreeNode(p)).ToList();
			//try to load in either npcs or categories, and create corresponding nodes for each.
			var nodes = models.Select(m => m is CategoryModel ? new CategoryTreeNode<Npc, NpcTreeNode>((CategoryModel)m, path) :
																(ModelTreeNode)new NpcTreeNode((Npc)m))
							   .ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = new List<IModel>();
			var includeModels = new List<IncludeModel>();

			foreach (var node in tree)
			{
				if (node is NpcTreeNode)
				{
					var npcTreeNode = (NpcTreeNode)node;
					var npc = npcTreeNode.Model as Npc;
					var lootEntries = npcTreeNode.LootEntryContainerNode.GetChildModels()
																		.Cast<LootEntry>()
																		.ToList();

					npc.LootEntries = lootEntries;
					models.Add(npc);
				}
				else if (node is CategoryTreeNode<Npc, NpcTreeNode>)
				{
					var catTreeNode = (CategoryTreeNode<Npc, NpcTreeNode>)node;
					var cat = catTreeNode.Model as CategoryModel;
					var childModels = catTreeNode.GetChildModels();

					cat.Includes.Clear();

					foreach (var child in childModels)
					{
						var includeModel = (IncludeModel)child;

						includeModel.ParentPath = path;

						cat.Includes.Add(includeModel.RelativePath);

						includeModels.Add(includeModel);
					}

					//write this category and include information.
					models.Add(cat);
				}
			}

			includeModels.ThrowOnDuplicateIncludes();

			//save includes
			foreach (var im in includeModels)
				im.Save();

			var json = JsonConvert.SerializeObject(models, Formatting.Indented);
			File.WriteAllText(path, json);
		}
	}
}