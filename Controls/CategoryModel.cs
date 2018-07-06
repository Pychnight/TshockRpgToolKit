using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CategoryModel : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Category";

		[Browsable(false)]
		//[Category("Basic Properties")]
		//[DisplayName("Category")]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		[Category("Basic Properties")]
		[JsonProperty(Order = 0)]
		public string Category { get => Name; set => Name = value; }

		[Browsable(false)]
		[JsonProperty(Order = 1)]
		public List<string> Includes { get; set; } = new List<string>();

		//[Browsable(false)]
		//public List<IncludeModel> IncludeModels { get; set; } = new List<IncludeModel>();

		[Browsable(false)]
		public string BasePath { get; set; }

		public CategoryModel()
		{
		}

		public CategoryModel(CategoryModel other)
		{
			Name = other.Name;
			BasePath = other.BasePath;
			Includes = other.Includes.ToList();
		}

		//originally was void, but we're updating this for ModelTreeEditor...and need the loaded models.
		public List<TModel> LoadIncludes<TModel>(string basePath) where TModel : IModel, new()
		{
			throw new NotImplementedException("Needs to be redone.");

			var models = new List<TModel>();

			foreach(var inc in Includes)
			{
				var includeModel = new IncludeModel(basePath, inc);
				includeModel.Load<TModel>();
				//disabled during conversion to Npcsx ->> // IncludeModels.Add(includeModel);
			}

			return models;
		}

		/// <summary>
		/// kludge to ensure Includes list matches whats been set in the tree nodes, before serialization.
		/// </summary>
		internal void RefreshIncludes(BoundTreeNode node)
		{
			//Includes.Clear();
			//Includes = IncludeModels.Select(im => im.Name).ToList();

			var children = node.Nodes.Cast<BoundTreeNode>();
			Includes = children.Select(c => ( (IncludeModel)c.BoundObject ).Name).ToList();
		}


		object ICloneable.Clone()
		{
			return new CategoryModel(this);
		}
	}
}
