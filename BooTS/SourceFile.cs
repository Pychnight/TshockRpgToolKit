using System;
using System.IO;

namespace BooTS
{
	/// <summary>
	/// Lightweight wrapper of a filepath, its update time, and whether it exists at all. 
	/// </summary>
	internal class SourceFile : IEquatable<SourceFile>
	{
		public string FilePath { get; }
		public bool Exists => File.Exists(FilePath);
		public DateTime LastUpdated => Exists ? File.GetLastWriteTime(FilePath) : DateTime.Now;

		internal SourceFile(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("filePath may not be null or whitespace.");

			FilePath = filePath;
		}

		public override string ToString() => $"{FilePath}";// (LastUpdated:{LastUpdated.ToFileTime()})";

		public bool Equals(SourceFile other) => FilePath == other.FilePath;

		public override bool Equals(object obj)
		{
			if (obj is SourceFile)
				return Equals(obj as SourceFile);
			else
				return false;
		}

		public override int GetHashCode() => FilePath.GetHashCode();
	}
}
