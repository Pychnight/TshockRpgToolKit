using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class IncludeTreeNode<TModel,TNode> : ModelTreeStaticContainerNode where TModel : IModel, new()
																				where TNode : ModelTreeNode, new()
	{
		public IncludeTreeNode() : base()
		{
			ImageIndex = SelectedImageIndex = 1;

			CanEditModel = true;
			CanAdd = true;
			CanDelete = true;
			CanDrag = true;
		}

		//public override void AddChild(ModelTreeNode node)
		//{
		//	base.AddChild(node);
		//}

		public override void AddChildModel(IModel model)
		{
			////should be typed to something more specific
			//var node = new IncludeTreeNode<TModel>()
			//{
			//	Model = model
			//};

			//node.AddDefaultChildNodesHack();

			////add to tree
			//Nodes.Add(node);
		}

		//public override void AddChildModels(IList<IModel> models)
		//{
		//	base.AddChildModels(models);
		//}

		//public override void AddDefaultChildNodesHack()
		//{
		//	base.AddDefaultChildNodesHack();
		//}

		public override ModelTreeNode AddItem()
		{
			var model = new TModel();
			var node = new TNode();

			node.Model = model;

			AddSibling(node);

			return node;
		}

		//public override void AddSibling(ModelTreeNode node)
		//{
		//	base.AddSibling(node);
		//}

		//public override object Clone()
		//{
		//	return base.Clone();
		//}

		//public override ModelTreeNode Copy()
		//{
		//	return base.Copy();
		//}

		//public override IList<IModel> GetChildModels()
		//{
		//	return base.GetChildModels();
		//}

		//public override string ToString()
		//{
		//	return base.ToString();
		//}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is TNode || node is IncludeTreeNode<TModel,TNode>;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( CanAcceptDraggedNode(draggedNode) )
			{
				if(draggedNode is TNode)
				{
					AddChild(draggedNode);
					return true;
				}
				else if(draggedNode is IncludeTreeNode<TModel,TNode>)
				{
					AddSibling(draggedNode);
					return true;
				}
			}

			return false;
		}

		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//don't allow
		}
	}
}
