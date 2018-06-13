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
	}
}
