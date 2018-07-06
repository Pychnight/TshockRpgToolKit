using RpgToolsEditor.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models
{
	public static class ModelTreePersistance
	{
		public static List<IModel> Load<T>(string fileName) where T : IModel, new()
		{
			//var ctx = CreateMockContext();
			var json = File.ReadAllText(fileName);
			//var items = JsonConvert.DeserializeObject<List<IModel>>(json, new IModelConverter<T>());
			List<IModel> items = null;
			
			return items;
		}

		public static void Save(List<IModel> models, string fileName)
		{
			var json = JsonConvert.SerializeObject(models,Formatting.Indented);
			File.WriteAllText(fileName, json);
		}

		public static List<BoundTreeNode> LoadTree<TModel>(string fileName) where TModel : IModel, new()
		{
			var nodes = new List<BoundTreeNode>();
			var models = Load<TModel>(fileName);
			
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
					//foreach( var inc in cm.IncludeModels)
					//{
					//	var includeNode = new BoundTreeNode();
					//	node.Nodes.Add(includeNode);
					//	includeNode.BoundObject = inc;

					//	//create tree nodes for Npcs...
					//	foreach( var item in inc.Items )
					//	{
					//		var childNode = new BoundTreeNode();

					//		childNode.BoundObject = item;
					//		includeNode.Nodes.Add(childNode);
					//	}
					//}
				}
				
				nodes.Add(node);
			}
			
			return nodes;
		}

		public static void SaveTree(IList<BoundTreeNode> nodes, string fileName)// where TModel : IModel, new()
		{
			var categoryNodes = nodes.Where(n => n.BoundObject is CategoryModel).Select(n => n);

			//update include strings
			foreach( var catNode in categoryNodes )
			{
				var model = catNode.BoundObject as CategoryModel;
				model.RefreshIncludes(catNode);
			}

			//save direct items first
			var models = nodes.Select(n => n.BoundObject).ToList();
			Save(models, fileName);

			//now write include files...
			var baseDirectory = Path.GetDirectoryName(fileName);

			foreach(var catNode in categoryNodes)
			{
				foreach( var node in catNode.Nodes)
				{
					var incNode = (BoundTreeNode)node;
					var itemNodes = incNode.Nodes.Cast<BoundTreeNode>().ToList();
					var items = itemNodes.Select(i => i.BoundObject).ToList();

					var incName = ((IncludeModel)(incNode.BoundObject)).Name;
					var incPath = Path.Combine(baseDirectory, incName);
					var json = JsonConvert.SerializeObject(items,Formatting.Indented);
					File.WriteAllText(incPath, json);
				}
			}
		}
	}
}
