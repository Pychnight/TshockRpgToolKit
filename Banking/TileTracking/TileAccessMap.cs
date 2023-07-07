using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Banking.TileTracking
{
	/// <summary>
	/// Used to flag changes to the world tilemap in a compact form.  
	/// </summary>
	[Serializable]
	public class TileAccessMap
	{
		BitArray tileBits;

		public int Columns { get; private set; }
		public int Rows { get; private set; }

		public bool this[int x, int y]
		{
			get => tileBits[(y * Columns) + x];
			set => tileBits[(y * Columns) + x] = value;
		}

		public TileAccessMap(int columns, int rows)
		{
			Columns = columns;
			Rows = rows;

			tileBits = new BitArray(columns * rows);
		}

		/// <summary>
		/// Loads the TileAccessMap from disk.
		/// </summary>
		/// <param name="fileName">Filepath.</param>
		/// <returns>TileAccessMap.</returns>
		public static TileAccessMap Load(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Open))
			using (var gs = new GZipStream(fs, CompressionMode.Decompress))
			{
				var bf = new BinaryFormatter();
				return (TileAccessMap)bf.Deserialize(gs);
			}
		}

		/// <summary>
		/// Saves a TileAccessMap to disk.
		/// </summary>
		/// <param name="fileName">Filepath.</param>
		public void Save(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Create))
			using (var gs = new GZipStream(fs, CompressionLevel.Fastest))
			{
				var bf = new BinaryFormatter();
				bf.Serialize(gs, this);
			}
		}
	}
}
