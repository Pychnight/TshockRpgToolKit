using CustomNpcsEdit.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Models
{
	public static class NpcTree
	{
		public static List<IModel> Load(string fileName)
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			var items = JsonConvert.DeserializeObject<List<IModel>>(json, new IModelConverter<Npc>());

			return items;
		}

		public static List<BoundTreeNode> LoadTree<TModel>(string fileName) where TModel : IModel, new()
		{
			var nodes = new List<BoundTreeNode>();
			var models = Load(fileName);
			
			foreach(var m in models)
			{
				var node = new BoundTreeNode();
				node.BoundObject = m;// as CategoryModel;

				//load in includes
				if(node.BoundObject is CategoryModel)
				{
					var cm = node.BoundObject as CategoryModel;
					cm.LoadIncludes<TModel>(fileName);

					//create tree nodes for includes...
					foreach( var inc in cm.IncludeModels)
					{
						var includeNode = new BoundTreeNode();
						node.Nodes.Add(includeNode);
						includeNode.BoundObject = inc;

						//create tree nodes for Npcs...
						foreach( var item in inc.Items )
						{
							var childNode = new BoundTreeNode();

							childNode.BoundObject = item;
							includeNode.Nodes.Add(childNode);
						}
					}
				}
				
				nodes.Add(node);
			}
			
			return nodes;
		}
	}
}
