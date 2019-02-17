//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using TEdit.Geometry.Primitives;
//using GalaSoft.MvvmLight;

using Microsoft.Xna.Framework;

namespace Corruption.TEdit //TEditXNA.Terraria.Objects
{
	public class ItemProperty : ITile //: ObservableObject, ITile
	{
		public ItemProperty()
		{
			Name = "UNKNOWN";
			Id = 0;
		}

		private int _maxStackSize;
		private int _id;
		private string _name;
		private float _scale;
		private Vector2Short _size;
		private Vector2Short _uV;

		public Vector2Short UV
		{
			get { return _uV; }
			set { _uV = value; }
		}

		public Vector2Short Size
		{
			get { return _size; }
			set { _size = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public float Scale
		{
			get { return _scale; }
			set { _scale = value; }
		}

		//private WriteableBitmap _image;
		//public WriteableBitmap Image
		//{
		//	get { return _image; }
		//	set { Set("Image", ref _image, value); }
		//}

		public Color Color
		{
			get { return Color.Transparent; }
		}

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public int MaxStackSize
		{
			get { return _maxStackSize; }
			set { _maxStackSize = value; }
		}
	}
}