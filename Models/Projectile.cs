using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace CustomNpcsEdit.Models
{
	[DefaultProperty("Name")]
	[JsonObject(MemberSerialization.OptIn)]
	public class Projectile : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Projectile";

		[Category("Basic Properties")]
		[Description("The name of the Projectile.")]
		[JsonProperty(Order = 0)]
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		[Category("Basic Properties")]
		[Description("Path to a Boo script that runs custom logic for various Projectile hook points.")]
		[Editor(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[JsonProperty(Order = 1)]
		public string ScriptPath { get; set; } = "";

		[Category("Basic Properties")]
		[Description("The Terraria Projectile type this Custom Projectile is based upon.")]
		[JsonProperty(Order = 2)]
		public int BaseType { get; set; }
				
		private ProjectileBaseOverride baseOverride = new ProjectileBaseOverride();

		[Browsable(false)]
		[JsonProperty("BaseOverride", Order = 3)]
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

	//[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject(MemberSerialization.OptIn)]
	public class ProjectileBaseOverride
	{
		[JsonProperty]
		public int? AiStyle { get; set; }

		[JsonProperty]
		public float[] Ai { get; set; }

		[JsonProperty]
		public int? Damage { get; set; }

		[JsonProperty]
		public int? KnockBack { get; set; }

		[JsonProperty]
		public bool? Friendly { get; set; }

		[JsonProperty]
		public bool? Hostile { get; set; }

		[JsonProperty]
		public int? MaxPenetrate { get; set; }

		[JsonProperty]
		public int? TimeLeft { get; set; }

		//[JsonProperty]
		//public int? Width { get; set;}

		//[JsonProperty]
		//public int? Height { get; set;}

		[JsonProperty]
		public bool? Magic { get; set; }

		[JsonProperty]
		public float? Light { get; set; }

		[JsonProperty]
		public bool? Thrown { get; set; }

		[JsonProperty]
		public bool? Melee { get; set; }

		[JsonProperty]
		public bool? ColdDamage { get; set; }

		[JsonProperty]
		public bool? TileCollide { get; set; }

		[JsonProperty]
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

			AiStyle = other.AiStyle;
			Ai = other.Ai?.ToArray() ?? null;
			Damage = other.Damage;
			KnockBack = other.KnockBack;
			Friendly = other.Friendly;
			Hostile = other.Hostile;
			MaxPenetrate = other.MaxPenetrate;
			TimeLeft = other.TimeLeft;
			Magic = other.Magic;
			Light = other.Light;
			Thrown = other.Thrown;
			Melee = other.Melee;
			ColdDamage = other.ColdDamage;
			TileCollide = other.TileCollide;
			IgnoreWater = other.IgnoreWater;
		}

		//public override string ToString()
		//{
		//	return "";
		//}
	}
}
