using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class InvasionModelTree : ModelTree
	{
		public override ModelTreeNode CreateDefaultItem()
		{
			var model = new Invasion();
			var node = new InvasionTreeNode(model);
			return node;
		}

		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var model = new Invasion();
			var node = new InvasionTreeNode(model);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var loader = new CategoryLoader<Invasion>();
			var models = loader.ParseCategorysAndModels(json);

			//try to load in either npcs or categories, and create corresponding nodes for each.
			var nodes = models.Select(m => m is CategoryModel ? new CategoryTreeNode<Invasion, InvasionTreeNode>((CategoryModel)m, path) :
																(ModelTreeNode)new InvasionTreeNode((Invasion)m))
							   .ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = new List<IModel>();
			var includeModels = new List<IncludeModel>();

			foreach (var node in tree)
			{
				if (node is InvasionTreeNode)
				{
					var invasionTreeNode = (InvasionTreeNode)node;
					var invasion = invasionTreeNode.Model as Invasion;
					var waves = invasionTreeNode.WavesContainerNode.GetChildModels()
																	.Cast<Wave>()
																	.ToList();

					invasion.Waves = waves;
					models.Add(invasion);
				}
				else if (node is CategoryTreeNode<Invasion, InvasionTreeNode>)
				{
					var catTreeNode = (CategoryTreeNode<Invasion, InvasionTreeNode>)node;
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