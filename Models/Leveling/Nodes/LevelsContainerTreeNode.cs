using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.Leveling
{
	public class LevelsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public LevelsContainerTreeNode()
		{
			Text = "Levels";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override void AddChildModel(IModel model)
		{
			var node = new LevelTreeNode()
			{
				Model = model
			};

			node.AddDefaultChildNodesHack();

			//add to tree
			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new Level();
			var node = new LevelTreeNode()
			{
				Model = item
			};

			node.AddDefaultChildNodesHack();
			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is LevelTreeNode;
			return result;
		}
	}
}