using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.TEdit
{
	public abstract class SchematicReader
	{
		public abstract Schematic Read(BinaryReader reader, string name, int version);
	}
}
