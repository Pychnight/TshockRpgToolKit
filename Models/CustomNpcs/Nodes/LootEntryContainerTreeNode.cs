using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class LootEntrysContainerTreeNode : ModelTreeStaticContainerNode
	{
		public LootEntrysContainerTreeNode()
		{
			Text = "Loot";
			ImageIndex = SelectedImageIndex = 1;
		}
		
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