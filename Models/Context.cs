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
	//[JsonObject(MemberSerialization.OptIn)]
	//[TypeConverter(typeof(ExpandableObjectConverter))]
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

		public ProjectileBaseOverride()
		{
		}

		public ProjectileBaseOverride(ProjectileBaseOverride other)
		{
			if( other == null )
				return;

			AiStyle			= other.AiStyle;
			Ai				= other.Ai?.ToArray() ?? null;
			Damage			= other.Damage;
			KnockBack		= other.KnockBack;
			Friendly		= other.Friendly;
			Hostile			= other.Hostile;
			MaxPenetrate	= other.MaxPenetrate;
			TimeLeft		= other.TimeLeft;
			Magic			= other.Magic;
			Light			= other.Light;
			Thrown			= other.Thrown;
			Melee			= other.Melee;
			ColdDamage		= other.ColdDamage;
			TileCollide		= other.TileCollide;
			IgnoreWater		= other.IgnoreWater;
		}

		//public override string ToString()
		//{
		//	return "";
		//}
	}

	//public abstract class Context<TItem>
	//{
	//	public string Filename { get; set; }


	//	public abstract Context<TItem> Load(string fileName);
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


		private ProjectileBaseOverride baseOverride = new ProjectileBaseOverride();

		//[JsonProperty("BaseOverride", Order = 3)]
		//[Category("Override Properties")]
		[Browsable(false)]
		public ProjectileBaseOverride ProjectileBaseOverride
		{
			get => baseOverride;
			set => baseOverride = value;
		}

		[Category("Override Properties")]
		public int? AiStyle
		{
			get => baseOverride.AiStyle;
			set => baseOverride.AiStyle = value;
		}

		[Category("Override Properties")]
		public float[] Ai
		{
			get => baseOverride.Ai;
			set => baseOverride.Ai = value;
		}

		[Category("Override Properties")]
		public int? Damage
		{
			get => baseOverride.Damage;
			set => baseOverride.Damage = value;
		}

		[Category("Override Properties")]
		public int? KnockBack
		{
			get => baseOverride.KnockBack;
			set => baseOverride.KnockBack = value;
		}

		[Category("Override Properties")]
		public bool? Friendly
		{
			get => baseOverride.Friendly;
			set => baseOverride.Friendly = value;
		}

		[Category("Override Properties")]
		public bool? Hostile
		{
			get => baseOverride.Hostile;
			set => baseOverride.Hostile = value;
		}

		[Category("Override Properties")]
		public int? MaxPenetrate
		{
			get => baseOverride.MaxPenetrate;
			set => baseOverride.MaxPenetrate = value;
		}

		[Category("Override Properties")]
		public int? TimeLeft
		{
			get => baseOverride.TimeLeft;
			set => baseOverride.TimeLeft = value;
		}

		//[JsonProperty]
		//public int? Width { get; set;}

		//[JsonProperty]
		//public int? Height { get; set;}

		[Category("Override Properties")]
		public bool? Magic
		{
			get => baseOverride.Magic;
			set => baseOverride.Magic = value;
		}

		[Category("Override Properties")]
		public float? Light
		{
			get => baseOverride.Light;
			set => baseOverride.Light = value;
		}

		[Category("Override Properties")]
		public bool? Thrown
		{
			get => baseOverride.Thrown;
			set => baseOverride.Thrown = value;
		}

		[Category("Override Properties")]
		public bool? Melee
		{
			get => baseOverride.Melee;
			set => baseOverride.Melee = value;
		}

		[Category("Override Properties")]
		public bool? ColdDamage
		{
			get => baseOverride.ColdDamage;
			set => baseOverride.ColdDamage = value;
		}

		[Category("Override Properties")]
		public bool? TileCollide
		{
			get => baseOverride.TileCollide;
			set => baseOverride.TileCollide = value;
		}

		[Category("Override Properties")]
		public bool? IgnoreWater
		{
			get => baseOverride.IgnoreWater;
			set => baseOverride.IgnoreWater = value;
		}
		
		public Projectile()
		{
		}

		public Projectile(Projectile other)
		{
			Name = other.Name;
			BaseType = other.BaseType;
			ScriptPath = other.ScriptPath;
			ProjectileBaseOverride = new ProjectileBaseOverride(other.ProjectileBaseOverride);
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
