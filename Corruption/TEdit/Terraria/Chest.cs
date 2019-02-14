using System;
using System.Collections.ObjectModel;
//using GalaSoft.MvvmLight;
//using TEditXNA.Terraria.Objects;
//using TEditXna.ViewModel;


namespace Corruption.TEdit //TEditXNA.Terraria
{
	//just a dummy version for development/testing purposes...
	//public class Item
	//{
	//	public int StackSize;
	//	public int NetId;
	//	public int Prefix;

	//	public Item Copy()
	//	{
	//		return (Item)this.MemberwiseClone();
	//	}
	//}
	
	[Serializable]
	public class Chest //: ObservableObject
	{
		public static int MaxItems = 40;

		public Chest()
		{
			for( int i = 0; i < MaxItems; i++ )
			{
				_items.Add(new Item());
			}
		}
		public Chest(int x, int y)
			: this()
		{
			_x = x;
			_y = y;

		}

		public Chest(int x, int y, string name)
			: this()
		{
			_x = x;
			_y = y;
			_name = name;
		}

		/// <summary>
		/// Creates a Corruption.TEdit.Chest from a Terraria.Chest.
		/// </summary>
		/// <param name="tChest"></param>
		public Chest(Terraria.Chest tChest) : this( tChest.x, tChest.y, tChest.name)
		{
			for( int i = 0; i < MaxItems; i++ )
			{
				var src = tChest.item[i];

				if(src!=null)
				{
					var dst = _items[i];

					dst.NetId = src.netID;
					dst.Prefix = src.prefix;
					dst.StackSize = src.stack;
				}
			}
		}
		
		private int _x;
		private int _y;

		private string _name = string.Empty;
		//private int _chestId = -1;

		//public int ChestId
		//{
		//	get
		//	{
		//		WorldViewModel wvm = ViewModelLocator.WorldViewModel;
		//		World world = wvm.CurrentWorld;
		//		var uvX = world.Tiles[X, Y].U;
		//		var uvY = world.Tiles[X, Y].V;
		//		var type = world.Tiles[X, Y].Type;
		//		foreach( ChestProperty prop in World.ChestProperties )
		//		{
		//			if( prop.TileType == type && prop.UV.X == uvX && prop.UV.Y == uvY )
		//			{
		//				_chestId = prop.ChestId;
		//				break;
		//			}
		//		}
		//		return _chestId;
		//	}
		//	set
		//	{
		//		foreach( ChestProperty prop in World.ChestProperties )
		//		{
		//			if( prop.ChestId == value )
		//			{
		//				WorldViewModel wvm = ViewModelLocator.WorldViewModel;
		//				World world = wvm.CurrentWorld;
		//				int rowNum = 2, colNum = 2;
		//				// Chests are 2 * 2, dressers are 2 * 3.
		//				if( prop.TileType == 88 )
		//				{
		//					colNum = 3;
		//				}
		//				for( int i = 0; i < colNum; ++i )
		//				{
		//					for( int j = 0; j < rowNum; ++j )
		//					{
		//						world.Tiles[X + i, Y + j].U = (short)( prop.UV.X + 18 * i );
		//						world.Tiles[X + i, Y + j].V = (short)( prop.UV.Y + 18 * j );
		//						world.Tiles[X + i, Y + j].Type = prop.TileType;
		//					}
		//				}
		//				Set("ChestId", ref _chestId, value);
		//				break;
		//			}
		//		}
		//	}
		//}

		public string Name { get => _name; set => _name = value; }
		public int X { get => _x; set => _x = value; }
		public int Y { get => _y; set => _y = value; }
		
		private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
		public ObservableCollection<Item> Items
		{
			get { return _items; }
		}

		public Chest Copy()
		{
			var chest = new Chest(_x, _y);
			chest.Name = Name;
			//chest.Items.Clear();
			for( int i = 0; i < MaxItems; i++ )
			{
				if( Items.Count > i )
				{
					//throw new NotImplementedException("fixme");
					chest.Items[i] = Items[i].Copy();
				}
				else
				{
					chest.Items[i] = new Item();
				}
			}

			return chest;
		}

		public override string ToString()
		{
			return $"[Chest: ({X},{Y})]";
		}
	}
}
