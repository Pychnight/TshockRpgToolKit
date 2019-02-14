using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// The location of data or an object within a text file. This is generally used for error reporting. 
	/// </summary>
	public class FilePosition
	{
		public string FilePath { get; set; } = "";
		public int Line { get; set; }
		public int Column { get; set; }

		public FilePosition() : this(null) { }

		public FilePosition(string filePath, int line = 0, int column = 0 )
		{
			FilePath = filePath ?? "";
			Line = line;
			Column = column;
		}

		public override string ToString() => $"{FilePath} {Line},{Column}";
	}
}
