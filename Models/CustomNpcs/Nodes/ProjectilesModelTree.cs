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
			var projectiles = JsonConvert.DeserializeObject<List<Projectile>>(json);
			var nodes = projectiles.Select(p => (ModelTreeNode)new ProjectileTreeNode(p)).ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = tree.Select(n => (Projectile)n.Model).ToList();	
			
			var json = JsonConvert.SerializeObject(models);
			File.WriteAllText(path, json);
		}
	}
}