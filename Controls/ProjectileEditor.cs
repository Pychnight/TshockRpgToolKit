using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomNpcsEdit.Models;

namespace CustomNpcsEdit.Controls
{
	public partial class ProjectileEditor : UserControl
	{
		ProjectileContext projectileContext { get; set; }
		
		public ProjectileEditor()
		{
			InitializeComponent();
			
			projectileContext = ProjectileContext.CreateMockContext();
			
			listBoxItems.DisplayMember = "Name";
			listBoxItems.ValueMember = "Name";
			listBoxItems.DataSource = projectileContext;
		}

		private void listBoxItems_SelectedValueChanged(object sender, EventArgs e)
		{
			propertyGridItemEditor.SelectedObject = listBoxItems.SelectedItem;
		}

		private void toolStripButtonAddItem_Click(object sender, EventArgs e)
		{
			var item = new Projectile();
			projectileContext.Add(item);
		}
		
		private void toolStripButtonCopy_Click(object sender, EventArgs e)
		{
			const string suffix = "(Copy)";

			var index = listBoxItems.SelectedIndex;

			if( index > -1 )
			{
				var copy = new Projectile((Projectile)listBoxItems.SelectedItem);

				if(!copy.Name.EndsWith(suffix))
					copy.Name = copy.Name + suffix;

				projectileContext.Insert(++index,copy);
			}
		}

		private void toolStripButtonDeleteItem_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if(index>-1)
			{
				projectileContext.RemoveAt(index);
			}
		}

		private void toolStripButtonMoveUp_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if( index > 0  )
			{
				var item = projectileContext[index];

				projectileContext.RemoveAt(index);
				projectileContext.Insert(--index, item);
				listBoxItems.SelectedIndex = index;
			}
		}

		private void toolStripButtonMoveDown_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			var lastIndex = projectileContext.Count - 1;

			if( index > -1 && index < lastIndex )
			{
				var item = projectileContext[index];

				projectileContext.RemoveAt(index);

				index++;

				projectileContext.Insert(index, item);
				listBoxItems.SelectedIndex = index;
			}
		}
	}
}
