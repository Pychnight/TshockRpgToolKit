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

			ImageIndex = SelectedImageIndex = 2;
		}

		//public override void AddChild(ModelTreeNode node)
		//{
		//	base.AddChild(node);
		//}

		public override void AddChildModel(IModel model)
		{
			var node = new TNode()
			{
				Model = model
			};

			node.AddDefaultChildNodesHack();

			Nodes.Add(node);
		}

		public override IList<IModel> GetChildModels()
		{
			var models = new List<IModel>();

			foreach( var n in Nodes )
			{
				var modelTreeNode = n as TNode;

				if( modelTreeNode != null && modelTreeNode.Model != null )
				{
					//get loot.... this has been disabled, as its specific to npcs/loot, and causes failures for other types.
					//and its doing nothing, it seems.
					//var lootNode = modelTreeNode.Nodes[0] as LootEntrysContainerTreeNode;
					//var lootEntryModels = lootNode.GetChildModels();
					
					models.Add(modelTreeNode.Model);
				}
			}

			return models;
		}

		//public override void AddDefaultChildNodesHack()
		//{
		//	base.AddDefaultChildNodesHack();
		//}

		public override ModelTreeNode AddItem()
		{
			var model = new TModel();
			var node = new TNode();

			node.Model = model;
			node.AddDefaultChildNodesHack();

			//AddSibling(node);
			AddChild(node);

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
