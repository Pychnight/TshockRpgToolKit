using CustomNpcs.Projectiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace CustomNpcsEdit.Models
{
	//[TypeConverter(typeof(ExpandableObjectConverter))]
	//public class ProjectileBaseOverride
	//{
	//	public string Derp { get; set; }
	//	public int? Value { get; set; }
	//	public bool? IsValid { get; set; }

	//	public override string ToString()
	//	{
	//		return "";
	//	}
	//}

	//[JsonObject(MemberSerialization.OptIn)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class ProjectileBaseOverride
	{
		//[JsonProperty]
		public int? AiStyle { get; set; }

		//[JsonProperty]
		public float[] Ai { get; set; }

		//[JsonProperty]
		public int? Damage { get; set; }

		//[JsonProperty]
		public int? KnockBack { get; set; }

		//[JsonProperty]
		public bool? Friendly { get; set; }

		//[JsonProperty]
		public bool? Hostile { get; set; }

		//[JsonProperty]
		public int? MaxPenetrate { get; set; }

		//[JsonProperty]
		public int? TimeLeft { get; set; }

		//[JsonProperty]
		//public int? Width { get; set;}

		//[JsonProperty]
		//public int? Height { get; set;}

		//[JsonProperty]
		public bool? Magic { get; set; }

		//[JsonProperty]
		public float? Light { get; set; }

		//[JsonProperty]
		public bool? Thrown { get; set; }

		//[JsonProperty]
		public bool? Melee { get; set; }

		//[JsonProperty]
		public bool? ColdDamage { get; set; }

		//[JsonProperty]
		public bool? TileCollide { get; set; }

		//[JsonProperty]
		public bool? IgnoreWater { get; set; }

		/* 	[JsonProperty]
			public bool? Wet { get; set; } */

		//[JsonProperty]
		//public bool? Bobber { get; set; }

		//[JsonProperty]
		//public bool? Counterweight { get; set; }

		public override string ToString()
		{
			return "";
		}
	}

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
		public event PropertyChangedEventHandler PropertyChanged;

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
		[Editor(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string ScriptPath { get; set; } = "";

		//[JsonProperty(Order = 2)]
		[Category("Basic Properties")]
		[Description("The Terraria Projectile type this Custom Projectile is based upon.")]
		public int BaseType { get; set; }
		
		[Category("Basic Properties")]
		public bool BaseOverride { get; set; }

		//[JsonProperty("BaseOverride", Order = 3)]
		//[Category("Override Properties")]
		[Browsable(false)]
		public ProjectileBaseOverride ProjectileBaseOverride { get; set; } = new ProjectileBaseOverride();

		[Category("Override Properties")]
		public int? AiStyle { get; set; }

		[Category("Override Properties")]
		public float[] Ai { get; set; }

		[Category("Override Properties")]
		public int? Damage { get; set; }

		[Category("Override Properties")]
		public int? KnockBack { get; set; }

		[Category("Override Properties")]
		public bool? Friendly { get; set; }

		[Category("Override Properties")]
		public bool? Hostile { get; set; }

		[Category("Override Properties")]
		public int? MaxPenetrate { get; set; }

		[Category("Override Properties")]
		public int? TimeLeft { get; set; }

		//[JsonProperty]
		//public int? Width { get; set;}

		//[JsonProperty]
		//public int? Height { get; set;}

		[Category("Override Properties")]
		public bool? Magic { get; set; }

		[Category("Override Properties")]
		public float? Light { get; set; }

		[Category("Override Properties")]
		public bool? Thrown { get; set; }

		[Category("Override Properties")]
		public bool? Melee { get; set; }

		[Category("Override Properties")]
		public bool? ColdDamage { get; set; }

		[Category("Override Properties")]
		public bool? TileCollide { get; set; }

		[Category("Override Properties")]
		public bool? IgnoreWater { get; set; }
		
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
