using System;
using System.IO;
using CustomNpcsEdit.Models.Leveling;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Controls
{
	public class LevelingEditor : ObjectEditor
	{
		public LevelingEditor()
		{
		}

		protected override object OnCreateItem()
		{
			return new ClassDefinition();
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
			var classDef = JsonConvert.DeserializeObject<ClassDefinition>(txt);

			PropertyGrid.SelectedObject = classDef;
		}

		protected override void OnFileSave(string fileName)
		{
			var classDef = PropertyGrid.SelectedObject;

			if( classDef == null )
			{
				throw new Exception("ClassDefinition does not exist.");
				//return;
			}

			var json = JsonConvert.SerializeObject(classDef, Formatting.Indented);
			File.WriteAllText(fileName, json);
		}
	}
}
