using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class ProjectilesModelTree : ModelTree
	{
		public override ModelTreeNode CreateDefaultItem()
		{
			var model = new Projectile();
			var node = new ProjectileTreeNode(model);
			return node;
		}

		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var model = new Projectile();
			var node = new ProjectileTreeNode(model);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var loader = new CategoryLoader<Projectile>();
			var models = loader.ParseCategorysAndModels(json);

			//try to load in either npcs or categories, and create corresponding nodes for each.
			var nodes = models.Select(m => m is CategoryModel ? (ModelTreeNode)new CategoryTreeNode<Projectile, ProjectileTreeNode>((CategoryModel)m, path) :
																(ModelTreeNode)new ProjectileTreeNode((Projectile)m))
							   .ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = new List<IModel>();
			var includeModels = new List<IncludeModel>();

			foreach( var node in tree )
			{
				if( node is ProjectileTreeNode )
				{
					var projectileTreeNode = (ProjectileTreeNode)node;
					var projectile = projectileTreeNode.Model as Projectile;
					//var waves = invasionTreeNode.WavesContainerNode.GetChildModels()
					//												.Cast<Wave>()
					//												.ToList();

					//invasion.Waves = waves;
					models.Add(projectile);
				}
				else if( node is CategoryTreeNode<Projectile, ProjectileTreeNode> )
				{
					var catTreeNode = (CategoryTreeNode<Projectile, ProjectileTreeNode>)node;
					var cat = catTreeNode.Model as CategoryModel;
					var childModels = catTreeNode.GetChildModels();

					cat.Includes.Clear();

					foreach( var child in childModels )
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
			foreach( var im in includeModels )
				im.Save();

			var json = JsonConvert.SerializeObject(models, Formatting.Indented);
			File.WriteAllText(path, json);
		}
	}
}