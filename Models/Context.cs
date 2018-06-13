﻿using CustomNpcs.Projectiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Models
{
	//public abstract class Context<TItem>
	//{
	//	public string Filename { get; set; }


	//	public abstract Context<TItem> Load(string fileName);
	//}

	///// <summary>
	///// Wraps a CustomNpcs.ProjectileDefinition, and provides a shape suitable for our editor.
	///// </summary>
	//public class Projectile
	//{
		//internal ProjectileDefinition WrappedObject { get; set; }
		//public string Name { get => WrappedObject.Name; set => WrappedObject.Name = value; }
	//}

	//[JsonObject(MemberSerialization.OptIn)]
	[DefaultProperty("Name")]
	public class Projectile : INotifyPropertyChanged //: DefinitionBase, IDisposable
	{
		//[JsonProperty(Order = 0)]
		//public string Name { get; set; } = "";

		string name = "New Projectile";

		[Category("Basic Properties")]
		[Description("The name of the Projectile.")]
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		//[JsonProperty(Order = 1)]
		[Category("Basic Properties")]
		[Description("Path to a Boo script that runs custom logic for various Projectile hook points.")]
		public string ScriptPath { get; set; } = "";

		//[JsonProperty(Order = 2)]
		[Category("Basic Properties")]
		[Description("The Terraria Projectile type this Custom Projectile is based upon.")]
		public int BaseType { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		//[JsonProperty("BaseOverride", Order = 3)]
		//public BaseOverrideDefinition BaseOverride { get; set; } = new BaseOverrideDefinition();

		public Projectile()
		{
		}

		public Projectile(Projectile other)
		{
			Name = other.Name;
			BaseType = other.BaseType;
			ScriptPath = other.ScriptPath;
		}
	}

	public class ProjectileContext : BindingList<Projectile>
	{
		internal static ProjectileContext CreateMockContext()
		{
			var result = new ProjectileContext();

			for(var i =0;i<3;i++)
			{
				result.Add(new Projectile()
				{
					Name = $"Projectile{i}",
					ScriptPath = @"scripts/basicprojectile.boo",
					BaseType = i
				});
			}
			
			return result;
		}
	}
}
