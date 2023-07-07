using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.Linq;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class LootEntrysContainerTreeNode : ModelTreeStaticContainerNode
	{
		public LootEntrysContainerTreeNode() : base()
		{
			Text = "Loot";
			ImageIndex = SelectedImageIndex = 1;
			//CanEditModel = true;
		}

		//public LootEntrysContainerTreeNode(LootDefinition model) : this()
		//{
		//	Model = model;
		//}

		public override void AddChildModel(IModel model)
		{
			var node = new LootEntryTreeNode()
			{
				Model = model
			};

			node.AddDefaultChildNodesHack();

			//add to tree
			Nodes.Add(node);
		}

		public override IList<IModel> GetChildModels()
		{
			var models = new List<IModel>();
			var lootEntries = Nodes.Cast<LootEntryTreeNode>().Select(n => (LootEntry)n.Model);

			models.AddRange(lootEntries);

			return models;
		}

		public override ModelTreeNode AddItem()
		{
			var item = new LootEntry();
			var node = new LootEntryTreeNode()
			{
				Model = item
			};

			node.AddDefaultChildNodesHack();
			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is LootEntryTreeNode;
			return result;
		}
	}
}