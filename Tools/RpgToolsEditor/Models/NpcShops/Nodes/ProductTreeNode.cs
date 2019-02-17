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

			//Cant create node in default ctor, it fails on insert(during a copy)
			//Nodes.Add(new RequiredItemsContainerTreeNode());
		}

		/// <summary>
		/// HACK to let copying work. Unsure of cause, but we get a pattern of [null,<SomeNodeTypeHere>] in the Nodes collection.
		/// This causes Insert() to fail with a null ref exception. This lets us add the RequiredItemsContainerTreeNode() on our terms.
		/// </summary>
		public override void AddDefaultChildNodesHack()
		{
			//Cant create node in default ctor, it fails on insert(during a copy)
			var node = new RequiredItemsContainerTreeNode();
			Nodes.Add(node);
		}

		public override object Clone()
		{
			var clone = (ProductTreeNode)base.Clone();

			//clone.RequiredItemsTreeNode =

			//HACK - this lets copying work. Unsure of cause, but we get a pattern of [null,<SomeNodeTypeHere>] in the Nodes collection.
			//This causes Insert() to fail with a null ref exception. This tries to defeat it by removing the null
			//if(clone.Nodes.Count>1 && clone.Nodes[0]==null)
			//{
			//	//clone.Nodes.RemoveAt(0);
			//	clone.Nodes.Clear();
			//}
			
			return clone;
		}

		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//do nothing, this is not allowed for shop products.
		}
	}
}
