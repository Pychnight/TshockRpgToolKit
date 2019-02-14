//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework;
//using TEdit.Geometry.Primitives;
//using GalaSoft.MvvmLight;

namespace Corruption.TEdit //TEditXNA.Terraria.Objects
{
	public class FrameProperty : ITile //: ObservableObject, ITile
	{
		public FrameProperty()
		{
			Id = 0;
			Name = "Default";
			Color = Color.Magenta;
			UV = new Vector2Short(0, 0);
			//Anchor = FrameAnchor.None;
		}
		public FrameProperty(int id, string name, Vector2Short uv) : this()
		{
			Id = id;
			Name = name;
			UV = uv;
		}

		private string _name;
		private Vector2Short _uV;
		private int _id;
		//private FrameAnchor _anchor;

		private Color _color;
		//private WriteableBitmap _image;
		private string _variety;


		public string Variety { get; set; }

		//public FrameAnchor Anchor
		//{
		//	get { return _anchor; }
		//	set { Set("Anchor", ref _anchor, value); }
		//}

		public Color Color { get; set; }

		public int Id { get; set; }

		public Vector2Short UV { get; set; }

		public string Name { get; set; }

		//public WriteableBitmap Image
		//{
		//	get { return _image; }
		//	set { Set("Image", ref _image, value); }
		//}
	}
}