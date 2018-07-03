using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.NpcShops
{
	public class CommandsContainerTreeNode : ModelTreeStaticContainerNode
	{
		public CommandsContainerTreeNode()
		{
			Text = "ShopCommands";
			ImageIndex = SelectedImageIndex = 1;
		}

		public override void AddChildModel(IModel model)
		{
			var node = new CommandTreeNode()
			{
				Model = model
			};

			node.AddRequiredItemsContainerTreeNodeHack();
			Nodes.Add(node);
		}

		public override ModelTreeNode AddItem()
		{
			var item = new ShopCommand();
			var node = new CommandTreeNode()
			{
				Model = item
			};

			node.AddRequiredItemsContainerTreeNodeHack();
			Nodes.Add(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is CommandTreeNode;
			return result;
		}

		//public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		//{
		//	if( !CanAcceptDraggedNode(draggedNode) )
		//		return false;

		//	return true;
		//}

	}
}
