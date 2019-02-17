using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.CustomQuests
{
	public class QuestInfoModelTree : ModelTree
	{
		public override IList<ModelTreeNode> CreateTree()
		{
			var nodes = new List<ModelTreeNode>(1);
			var item = new QuestInfo();
			var node = new QuestInfoTreeNode(item);

			nodes.Add(node);

			return nodes;
		}

		public override IList<ModelTreeNode> LoadTree(string path)
		{
			var json = File.ReadAllText(path);
			var items = JsonConvert.DeserializeObject<List<QuestInfo>>(json);

			var nodes = items.Select(i => (ModelTreeNode)new QuestInfoTreeNode(i)).ToList();

			return nodes;
		}

		public override void SaveTree(IList<ModelTreeNode> tree, string path)
		{
			var items = tree.Select(n => (QuestInfo)n.Model).ToList();

			var json = JsonConvert.SerializeObject(items,Formatting.Indented);
			File.WriteAllText(path, json);
		}

		public override ModelTreeNode CreateDefaultItem()
		{
			var item = new QuestInfo();
			return new QuestInfoTreeNode(item);
		}
	}

	public class QuestInfoTreeNode : ModelTreeNode
	{
		public QuestInfoTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = false;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public QuestInfoTreeNode(QuestInfo model) : this()
		{
			Model = model;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is QuestInfoTreeNode;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if(CanAcceptDraggedNode(draggedNode))
			{
				draggedNode.Remove();
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}

	}
}
