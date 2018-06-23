using CustomNpcsEdit.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Controls
{
	public class NpcShopsEditor : ObjectEditor
	{
		public NpcShopsEditor()
		{
		}

		protected override object OnCreateItem()
		{
			return new NpcShopDefinition();
		}

		//we should never, ever call this, since NpcShopEditor is in single item mode.
		//protected override object OnCopyItem(object source)
		//{
		//	const string suffix = "(Copy)";

		//	var copy = new NpcShopDefinition((NpcShopDefinition)source);

		//	if( !copy.Name.EndsWith(suffix) )
		//		copy.Name = copy.Name + suffix;

		//	return copy;
		//}

		protected override void OnFileLoad(string fileName)
		{
			var txt = File.ReadAllText(fileName);
			var shopDef = JsonConvert.DeserializeObject<NpcShopDefinition>(txt);

			PropertyGrid.SelectedObject = shopDef;
		}

		protected override void OnFileSave(string fileName)
		{
			var shopDef = PropertyGrid.SelectedObject;

			if(shopDef==null)
			{
				throw new Exception("NpcShopDefinition does not exist.");
				//return;
			}

			var json = JsonConvert.SerializeObject(shopDef,Formatting.Indented);
			File.WriteAllText(fileName, json);
		}
	}
}
