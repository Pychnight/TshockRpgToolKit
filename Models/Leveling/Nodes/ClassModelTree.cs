using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Leveling
{
	public class ClassModelTree : ModelTree
	{
		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new Class();
			var node = new ClassTreeNode(item);
									
			return node;
		}

		public override IList<ModelTreeNode> CreateTree()
		{
			var root = new ClassTreeNode();
			var nodes = new List<ClassTreeNode>() { root };

			return nodes.Cast<ModelTreeNode>().ToList();
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var folderTreeNode = new FolderTreeNode();
			var directory = Path.GetDirectoryName(path);
			var classPaths = Directory.EnumerateFiles(directory, "*.class");

			folderTreeNode.Text = directory;

			foreach(var classPath in classPaths)
			{
				var json = File.ReadAllText(classPath);
				var item = JsonConvert.DeserializeObject<Class>(json);
				var classNode = new ClassTreeNode(item);

				folderTreeNode.AddChild(classNode);
			}
			
			return new List<ModelTreeNode>() { folderTreeNode };
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var folderTreeNode = (FolderTreeNode)tree.FirstOrDefault();
			var directory = Path.GetDirectoryName(path);

			foreach(var classTreeNode in folderTreeNode.Nodes.Cast<ModelTreeNode>().Select( n => (ClassTreeNode)n))
			{
				var levelContainer = classTreeNode.Nodes[0];
				var classModel = (Class)classTreeNode.Model;
				var levelModels = levelContainer.Nodes.Cast<LevelTreeNode>().Select(n => (Level)n.Model).ToList();

				classModel.LevelDefinitions = levelModels;

				var classPath = Path.Combine(directory, classModel.Name + ".class");

				var json = JsonConvert.SerializeObject(classModel);
				File.WriteAllText(classPath, json);
			}
		}
	}
}
