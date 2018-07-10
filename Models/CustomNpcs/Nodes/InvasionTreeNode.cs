using RpgToolsEditor.Controls;
using System.Linq;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class InvasionTreeNode : ModelTreeNode
	{
		public WavesContainerTreeNode WavesContainerNode => Nodes[0] as WavesContainerTreeNode;

		public InvasionTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public InvasionTreeNode(Invasion model) : this()
		{
			Model = model;

			var container = new WavesContainerTreeNode();

			container.AddChildModels(model.Waves.Cast<IModel>().ToList());

			Nodes.Add(container);
		}

		public override void AddDefaultChildNodesHack()
		{
			var container = new WavesContainerTreeNode();

			container.AddChildModels(( (Invasion)Model ).Waves.Cast<IModel>().ToList());

			Nodes.Add(container);
		}

		public override ModelTreeNode AddItem()
		{
			var model = new Invasion();
			var node = new InvasionTreeNode(model);

			this.AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is InvasionTreeNode;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( CanAcceptDraggedNode(draggedNode) )
			{
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}
}