using System.Collections.ObjectModel;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using TEdit.Geometry.Primitives;
//using GalaSoft.MvvmLight;
using Microsoft.Xna.Framework;

namespace Corruption.TEdit
{
	public class TileProperty : ITile //: ObservableObject, ITile
	{
		private Color _color;
		private int _id;
		private string _name;
		private bool _isFramed;
		//private Vector2Short _frameSize;
		private readonly ObservableCollection<FrameProperty> _frames = new ObservableCollection<FrameProperty>();
		//private bool _isSolid;
		//private bool _isSolidTop;
		//private bool _isLight;
		//private FramePlacement _placement;
		//private Vector2Short _textureGrid;

		//private bool _isGrass; /* Heathtech */
		//private bool _isPlatform; /* Heathtech */
		//private bool _isCactus; /* Heathtech */
		//private bool _hasFrameName;
		//private bool _isStone; /* Heathtech */
		//private bool _canBlend; /* Heathtech */
		//private int? _mergeWith; /* Heathtech */

		public TileProperty()
		{
			//_frameSize = new Vector2Short(1, 1);
			//_color = Colors.Magenta;
			_id = -1;
			_name = "UNKNOWN";
			_isFramed = false;
			//_isGrass = false; /* Heathtech */
			//_isPlatform = false; /* Heathtech */
			//_isCactus = false; /* Heathtech */
			//_hasFrameName = false;
			//_isStone = false; /* Heathtech */
			//_canBlend = false; /* Heathtech */
			//_mergeWith = null; /* Heathtech */
		}

		//public Vector2Short TextureGrid
		//{
		//	get { return _textureGrid; }
		//	set { Set("TextureGrid", ref _textureGrid, value); }
		//}
		//public FramePlacement Placement
		//{
		//	get { return _placement; }
		//	set { Set("Placement", ref _placement, value); }
		//}

		//public bool IsLight { get; set; }

		//public bool IsSolidTop { get; set; }

		public bool IsSolid { get; set; }

		public ObservableCollection<FrameProperty> Frames
		{
			get { return _frames; }
		}

		public TileProperty(int id, string name, Color color, bool isFramed = false)
		{
			_color = color;
			_id = id;
			_name = name;
			_isFramed = isFramed;
		}

		public Color Color { get; set; }

		public int Id { get; set; }

		public string Name { get; set; }

		//public Vector2Short FrameSize
		//{
		//	get { return _frameSize; }
		//	set { Set("FrameSize", ref _frameSize, value); }
		//}

		public bool IsFramed { get; set; }

		//private WriteableBitmap _image;
		//public WriteableBitmap Image
		//{
		//	get { return _image; }
		//	set { Set("Image", ref _image, value); }
		//}

		///* Heathtech */
		//public bool IsGrass { get; set; }

		///* Heathtech */
		//public bool IsPlatform { get; set; }

		///* Heathtech */
		//public bool IsCactus { get; set; }

		//public bool HasFrameName { get; set; }

		///* Heathtech */
		//public bool IsStone { get; set; }

		///* Heathtech */
		//public bool CanBlend { get; set; }

		///* Heathtech */
		//public int? MergeWith { get; set; }
	}
}