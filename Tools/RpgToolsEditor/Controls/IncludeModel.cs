﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace RpgToolsEditor.Controls
{
	//[JsonObject(MemberSerialization.OptIn)]
	public class IncludeModel : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		//string name = "";

		//[Browsable(false)]
		[DisplayName("Include")]
		[Description("Relative path to include file from master json file.")]
		public string Name
		{
			get => RelativePath;
			set
			{
				RelativePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		[Browsable(false)]
		public string ParentPath { get; set; }

		[Browsable(false)]
		public string ParentDirectory => Path.GetDirectoryName(ParentPath);

		[Browsable(false)]
		public string RelativePath { get; set; }

		[Browsable(false)]
		public string FullPath => Path.Combine(ParentDirectory, RelativePath);

		//public string FilePath
		//{
		//	get => Name;
		//	set => Name = value;
		//}

		[Browsable(false)]
		public List<IModel> Items { get; set; } = new List<IModel>();

		public IncludeModel() : this("", "include.json")
		{
		}

		public IncludeModel(string parentPath, string relativePath)
		{
			ParentPath = parentPath;
			RelativePath = relativePath;
		}

		public IncludeModel(IncludeModel other)
		{
			ParentPath = other.ParentPath;
			RelativePath = other.RelativePath;

			Items = other.Items.Select(i => (IModel)i.Clone()).ToList();
		}

		object ICloneable.Clone() => new IncludeModel(this);

		public List<TModel> Load<TModel>() where TModel : IModel, new()
		{
			//var path = Path.Combine(ParentDirectory, RelativePath);
			var path = FullPath;

			if (!File.Exists(path))
				return new List<TModel>();

			var json = File.ReadAllText(path);
			var items = JsonConvert.DeserializeObject<List<TModel>>(json);//, new IModelConverter<TModel>());

			var imodels = items.Cast<IModel>().ToList();

			Items = imodels;

			return items;
		}

		public void Save()
		{
			var path = Path.Combine(ParentDirectory, RelativePath);
			var dirName = Path.GetDirectoryName(path);

			Directory.CreateDirectory(dirName);

			var json = JsonConvert.SerializeObject(Items, Formatting.Indented);//, new IModelConverter<TModel>());
			File.WriteAllText(path, json);
		}
	}
}