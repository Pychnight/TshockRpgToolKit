﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models
{
	/// <summary>
	/// PropertyGrid's collection editor fails on Add, since String doesn't have a default, parameterless ctor. So we have this.
	/// .. sigh.
	/// </summary>
	[JsonConverter(typeof(StringHolderJsonConverter))]
	public class StringHolder : IStringHolder, ICloneable
	{
		public string Value { get; set; }

		public StringHolder() : this("") { }
		
		public StringHolder(string source)
		{
			Value = source;
		}

		public override string ToString()
		{
			return !string.IsNullOrWhiteSpace(Value) ? Value : "<Empty String>";
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}

	/// <summary>
	/// PropertyGrid's collection editor fails on Add, since String doesn't have a default, parameterless ctor. So we have this.
	/// This version provides design time support for drop downs with known Terraria Item Names.
	/// </summary>
	[JsonConverter(typeof(TerrariaItemStringHolderJsonConverter))]
	public class TerrariaItemStringHolder : IStringHolder, ICloneable
	{
		[TypeConverter(typeof(ItemNameStringConverter))]
		public string Value { get; set; }

		public TerrariaItemStringHolder() : this("") { }

		public TerrariaItemStringHolder(string source)
		{
			Value = source;
		}

		public override string ToString()
		{
			return !string.IsNullOrWhiteSpace(Value) ? Value : "<Empty Item String>";
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}

	/// <summary>
	/// Common interface for all StringHolders, which help support PropertyGrid interactions.
	/// </summary>
	public interface IStringHolder
	{
		string Value { get; set; }
	}
}
