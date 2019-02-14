using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.NpcShops
{
	public class RequiredItemsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public RequiredItemsContainerTreeNode() : base()
		{
			Text = "RequiredItems";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override void AddChildModel(IModel model)
		{
			var node = new RequiredItemTreeNode()
			{
				Model = model
			};

			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new RequiredItem();
			var node = new RequiredItemTreeNode()
			{
				Model = item
			};

			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is RequiredItemTreeNode;
			return result;
		}
	}
}
