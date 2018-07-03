using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.NpcShops
{
	public abstract class ProductTreeNode : ModelTreeNode
	{
		public RequiredItemsContainerTreeNode RequiredItemsTreeNode => Nodes[0] as RequiredItemsContainerTreeNode; //{ get; protected set; }

		public ProductTreeNode()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;

			//RequiredItemsTreeNode = new RequiredItemsContainerTreeNode();

			Nodes.Add(new RequiredItemsContainerTreeNode());
		}

		//public override object Clone()
		//{
		//	var clone = (NpcShopProductTreeNode)base.Clone();

		//	clone.RequiredItemsTreeNode = 

		//}

		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//do nothing, this is not allowed for shop products.
		}
	}
}
