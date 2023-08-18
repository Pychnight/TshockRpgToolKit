using System.IO;

namespace Corruption.TEdit
{
	public abstract class SchematicReader
	{
		public abstract Schematic Read(BinaryReader reader, string name, int version);
	}
}
