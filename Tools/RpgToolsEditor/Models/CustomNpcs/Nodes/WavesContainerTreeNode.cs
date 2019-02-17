using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class WavesContainerTreeNode : ModelTreeStaticContainerNode
	{
		public WavesContainerTreeNode()
		{
			Text = "Waves";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override void AddChildModel(IModel model)
		{
			var node = new WaveTreeNode()
			{
				Model = model
			};

			node.AddDefaultChildNodesHack();

			//add to tree
			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new Wave();
			var node = new WaveTreeNode()
			{
				Model = item
			};

			node.AddDefaultChildNodesHack();
			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is WaveTreeNode;
			return result;
		}
	}
}