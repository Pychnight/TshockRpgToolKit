using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	public class ProjectileTreeNode : ModelTreeNode
	{
		public ProjectileTreeNode() : base()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public ProjectileTreeNode(Projectile model) : this()
		{
			Model = model;
		}

		public override ModelTreeNode AddItem()
		{
			var model = new Projectile();
			var node = new ProjectileTreeNode(model);

			this.AddSibling(node);

			return node;
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node)
		{
			var result = node is ProjectileTreeNode ||
							node is CategoryTreeNode<Projectile, ProjectileTreeNode>;

			return result;
		}

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if (CanAcceptDraggedNode(draggedNode))
			{
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}
}
