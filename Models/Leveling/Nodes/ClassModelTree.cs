using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
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
			throw new NotImplementedException();
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			throw new NotImplementedException();
		}
	}
}
