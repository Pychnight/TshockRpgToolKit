using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;

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
			var projectiles = JsonConvert.DeserializeObject<List<Invasion>>(json);
			var nodes = projectiles.Select(p => (ModelTreeNode)new InvasionTreeNode(p)).ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var models = new List<Invasion>();

			foreach( var node in tree )
			{
				if( node is InvasionTreeNode )
				{
					var invasionTreeNode = (InvasionTreeNode)node;
					var invasion = invasionTreeNode.Model as Invasion;
					var waves = invasionTreeNode.WavesContainerNode.GetChildModels()
																	.Cast<Wave>()
																	.ToList();

					invasion.Waves = waves;
					models.Add(invasion);
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