using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.Linq;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class CategoryTreeNode<TModel,TNode> : ModelTreeStaticContainerNode where TNode : ModelTreeNode, new()
																				where TModel : IModel, new() 
	{
		public CategoryTreeNode() : base()
		{
			Text = "Category";
			ImageIndex = SelectedImageIndex = 1;

			CanEditModel = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public CategoryTreeNode(CategoryModel model, string basePath) : this()
		{
			Text = $"Category - {model.Name}";
			Model = model;

			//try to load in subnodes....
			var includeModels = model.Includes.Select( i=> new IncludeModel(basePath, i))
										.Cast<IModel>()
										.ToList();

			AddChildModels(includeModels);
			//model.LoadIncludes<TModel>(basePath);
		}

		public override void AddChildModel(IModel model)
		{
			var incModel = (IncludeModel)model;
			var childModels = incModel.Load<TModel>();
						
			var node = new IncludeTreeNode<TModel,TNode>()
			{
				Model = model
			};

			node.AddDefaultChildNodesHack();
			node.AddChildModels(childModels.Cast<IModel>().ToList());
			
			//add to tree
			Nodes.Add(node);
		}

		public override IList<IModel> GetChildModels()
		{
			var models = new List<IModel>();

			foreach( var n in Nodes )
			{
				var includeTreeNode = n as IncludeTreeNode<TModel,TNode>;
				var includeModel = includeTreeNode.Model as IncludeModel;
				
				includeModel.Items.Clear();
				
				if( includeTreeNode != null && includeTreeNode.Model != null )
				{
					//var childModels = includeTreeNode.Nodes.Cast<TNode>().Select(tn => tn.Model);
					var childModels = includeTreeNode.GetChildModels();

					includeModel.Items.AddRange(childModels);
					models.Add(includeModel);
					//models.AddRange(childModels);
				}
			}

			return models;
		}


		public override ModelTreeNode AddItem()
		{
			var item = new IncludeModel();
			//should be typed to something more specific
			var node = new IncludeTreeNode<TModel,TNode>()
			{
				Model = item
			};

			node.AddDefaultChildNodesHack();
			Nodes.Add(node);

			return node;
		}
		
		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			return node is TNode || //make sibling
					( node is CategoryTreeNode<TModel, TNode> && Parent == null ) || //make sibling
					( node is IncludeTreeNode<TModel, TNode>); //make child
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if( CanAcceptDraggedNode(draggedNode) )
			{
				if( draggedNode is IncludeTreeNode<TModel, TNode> )
					AddChild(draggedNode);
				else
					AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}
}